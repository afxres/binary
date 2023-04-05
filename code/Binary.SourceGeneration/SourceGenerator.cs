namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mikodev.Binary.SourceGeneration.Contexts;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

[Generator]
public sealed class SourceGenerator : IIncrementalGenerator
{
    private delegate object? TypeHandler(SourceGeneratorContext context, ITypeSymbol symbol);

    private static readonly ImmutableArray<TypeHandler> TypeHandlers = ImmutableArray.CreateRange(new TypeHandler[]
    {
        AttributeConverterContext.Invoke,
        AttributeConverterCreatorContext.Invoke,
        GenericConverterContext.Invoke,
        CollectionConverterContext.Invoke,
        TupleObjectConverterContext.Invoke,
        NamedObjectConverterContext.Invoke,
    });

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider.ForAttributeWithMetadataName(
            Constants.SourceGeneratorContextAttributeTypeName,
            (node, _) => node is TypeDeclarationSyntax,
            (context, _) => (TypeDeclarationSyntax)context.TargetNode);
        var provider = context.CompilationProvider.Combine(declarations.Collect());
        context.RegisterSourceOutput(provider, (context, source) => Invoke(source.Left, source.Right, context));
    }

    private static void Invoke(Compilation compilation, ImmutableArray<TypeDeclarationSyntax> declarations, SourceProductionContext context)
    {
        static DiagnosticDescriptor? Ensure(TypeDeclarationSyntax declaration, INamedTypeSymbol type)
        {
            if (declaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)) is false)
                return Constants.ContextTypeNotPartial;
            if (type.ContainingNamespace.IsGlobalNamespace)
                return Constants.ContextTypeNotInNamespace;
            if (type.ContainingType is not null)
                return Constants.ContextTypeNested;
            if (type.IsGenericType)
                return Constants.ContextTypeGeneric;
            return null;
        }

        if (declarations.IsDefaultOrEmpty)
            return;
        var cancellation = context.CancellationToken;
        var include = compilation.GetTypeByMetadataName(Constants.SourceGeneratorIncludeAttributeTypeName)?.ConstructUnboundGenericType();
        foreach (var declaration in declarations)
        {
            var model = compilation.GetSemanticModel(declaration.SyntaxTree);
            var type = model.GetDeclaredSymbol(declaration);
            if (type is null)
                continue;
            if (Ensure(declaration, type) is { } descriptor)
                context.ReportDiagnostic(Diagnostic.Create(descriptor, Symbols.GetLocation(type), new[] { type.Name }));
            else
                Invoke(compilation, context, type, include);
            cancellation.ThrowIfCancellationRequested();
        }
    }

    private static ImmutableDictionary<ITypeSymbol, AttributeData> GetIncludedTypes(SourceProductionContext context, INamedTypeSymbol type, INamedTypeSymbol? include)
    {
        var builder = ImmutableDictionary.CreateBuilder<ITypeSymbol, AttributeData>(SymbolEqualityComparer.Default);
        var attributes = type.GetAttributes();
        var cancellation = context.CancellationToken;
        foreach (var i in attributes)
        {
            cancellation.ThrowIfCancellationRequested();
            var attribute = i.AttributeClass;
            if (attribute is null || attribute.IsGenericType is false)
                continue;
            var definitions = attribute.ConstructUnboundGenericType();
            if (SymbolEqualityComparer.Default.Equals(definitions, include) is false)
                continue;
            var includedType = attribute.TypeArguments.Single();
            if (builder.ContainsKey(includedType) is false)
                builder.Add(includedType, i);
            else
                context.ReportDiagnostic(Diagnostic.Create(Constants.IncludeTypeDuplicated, Symbols.GetLocation(i), new[] { includedType.Name }));
            cancellation.ThrowIfCancellationRequested();
        }
        return builder.ToImmutable();
    }

    private static void Invoke(Compilation compilation, SourceProductionContext context, INamedTypeSymbol type, INamedTypeSymbol? include)
    {
        var includedTypes = GetIncludedTypes(context, type, include);
        var pending = new Queue<ITypeSymbol>(includedTypes.Keys);
        var handled = new Dictionary<ITypeSymbol, string?>(SymbolEqualityComparer.Default);
        var generator = new SourceGeneratorContext(type, compilation, context, pending);

        string? Handle(ITypeSymbol symbol)
        {
            if (Symbols.Validate(generator, symbol) is false)
                return null;
            var result = TypeHandlers.Select(h => h.Invoke(generator, symbol)).FirstOrDefault(x => x is not null);
            if (result is string target)
                return target;
            var diagnostic = ((Diagnostic?)result) ?? (includedTypes.TryGetValue(symbol, out var attribute)
                ? Diagnostic.Create(Constants.NoConverterGenerated, Symbols.GetLocation(attribute), new[] { symbol.Name })
                : null);
            if (diagnostic is not null)
                context.ReportDiagnostic(diagnostic);
            return null;
        }

        var cancellation = context.CancellationToken;
        while (pending.Count is not 0)
        {
            cancellation.ThrowIfCancellationRequested();
            var symbol = pending.Dequeue();
            if (handled.ContainsKey(symbol))
                continue;
            var result = Handle(symbol);
            handled.Add(symbol, result);
            cancellation.ThrowIfCancellationRequested();
        }

        AppendConverterCreators(context, generator, handled);
    }

    private static void AppendConverterCreators(SourceProductionContext context, SourceGeneratorContext generation, IReadOnlyDictionary<ITypeSymbol, string?> creators)
    {
        var builder = new StringBuilder();
        var cancellation = context.CancellationToken;
        builder.AppendIndent(0, $"namespace {generation.Namespace};");
        builder.AppendIndent();
        builder.AppendIndent(0, $"using _IDictionary = global::System.Collections.Immutable.ImmutableDictionary<global::System.Type, global::Mikodev.Binary.IConverterCreator>;");
        builder.AppendIndent(0, $"using _SDictionary = global::System.Collections.Immutable.ImmutableDictionary;");
        builder.AppendIndent(0, $"using _TDictionary = global::System.Collections.Generic.Dictionary<global::System.Type, global::Mikodev.Binary.IConverterCreator>;");
        builder.AppendIndent();
        builder.AppendIndent(0, $"partial class {generation.Name}");
        builder.AppendIndent(0, $"{{");
        builder.AppendIndent(1, $"public static _IDictionary ConverterCreators {{ get; }} = _SDictionary.CreateRange(new _TDictionary");
        builder.AppendIndent(1, $"{{");
        foreach (var i in creators)
        {
            cancellation.ThrowIfCancellationRequested();
            if (i.Value is not { } creator)
                continue;
            var key = Symbols.GetSymbolFullName(i.Key);
            builder.AppendIndent(2, $"{{ typeof({key}), new {creator}() }},");
            cancellation.ThrowIfCancellationRequested();
        }
        builder.AppendIndent(1, $"}});");
        builder.AppendIndent(0, $"}}");

        var code = builder.ToString();
        var file = $"{generation.HintNameUnit}.g.cs";
        context.AddSource(file, code);
    }
}
