namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mikodev.Binary.SourceGeneration.Contexts;
using System;
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
        EnumConverterContext.Invoke,
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
        var types = new List<INamedTypeSymbol>();
        foreach (var declaration in declarations)
        {
            var model = compilation.GetSemanticModel(declaration.SyntaxTree);
            var type = model.GetDeclaredSymbol(declaration);
            if (type is null)
                continue;
            if (Ensure(declaration, type) is { } descriptor)
                context.ReportDiagnostic(Diagnostic.Create(descriptor, Symbols.GetLocation(type), new object[] { Symbols.GetDiagnosticName(type) }));
            else
                types.Add(type);
            cancellation.ThrowIfCancellationRequested();
        }

        foreach (var i in types.GroupBy(x => x.Name, StringComparer.InvariantCulture))
        {
            var index = 0;
            foreach (var type in i.OrderBy(x => x.ContainingNamespace.ToDisplayString(), StringComparer.InvariantCulture))
            {
                var file = $"{i.Key}.{index}.g.cs";
                Invoke(compilation, context, file, type, include);
                index++;
            }
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
            if (Symbols.IsTypeSupported(includedType) is false)
                context.ReportDiagnostic(Diagnostic.Create(Constants.RequireSupportedTypeForIncludeAttribute, Symbols.GetLocation(i), new object[] { Symbols.GetDiagnosticName(includedType) }));
            else if (builder.ContainsKey(includedType))
                context.ReportDiagnostic(Diagnostic.Create(Constants.IncludeTypeDuplicated, Symbols.GetLocation(i), new object[] { Symbols.GetDiagnosticName(includedType) }));
            else
                builder.Add(includedType, i);
            cancellation.ThrowIfCancellationRequested();
        }
        return builder.ToImmutable();
    }

    private static void Invoke(Compilation compilation, SourceProductionContext context, string file, INamedTypeSymbol type, INamedTypeSymbol? include)
    {
        var includedTypes = GetIncludedTypes(context, type, include);
        var pending = new Queue<ITypeSymbol>(includedTypes.Keys);
        var handled = new Dictionary<ITypeSymbol, SymbolConverterContent?>(SymbolEqualityComparer.Default);
        var generator = new SourceGeneratorContext(type, compilation, context, pending);

        SymbolConverterContent? Handle(ITypeSymbol symbol)
        {
            if (Symbols.Validate(generator, symbol) is false)
                return null;
            var result = TypeHandlers.Select(h => h.Invoke(generator, symbol)).FirstOrDefault(x => x is not null);
            if (result is SymbolConverterContent target)
                return target;
            var diagnostic = ((Diagnostic?)result) ?? (includedTypes.TryGetValue(symbol, out var attribute)
                ? Diagnostic.Create(Constants.NoConverterGenerated, Symbols.GetLocation(attribute), new object[] { Symbols.GetDiagnosticName(symbol) })
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
            if (symbol.IsTupleType)
                symbol = ((INamedTypeSymbol)symbol).TupleUnderlyingType ?? symbol;
            if (handled.ContainsKey(symbol))
                continue;
            var result = Handle(symbol);
            handled.Add(symbol, result);
            cancellation.ThrowIfCancellationRequested();
        }

        AppendConverterCreators(context, file, type, handled.Values.OfType<SymbolConverterContent>());
    }

    private static void AppendConverterCreators(SourceProductionContext context, string file, INamedTypeSymbol type, IEnumerable<SymbolConverterContent> creators)
    {
        var builder = new StringBuilder();
        var cancellation = context.CancellationToken;
        var targets = creators.OrderBy(x => x.ConverterCreatorTypeName, StringComparer.CurrentCultureIgnoreCase).ToList();

        builder.AppendIndent(0, $"namespace {type.ContainingNamespace.ToDisplayString()};");
        builder.AppendIndent();
        builder.AppendIndent(0, $"partial class {type.Name}");
        builder.AppendIndent(0, $"{{");
        builder.AppendIndent(1, $"public static global::System.Collections.Immutable.ImmutableDictionary<global::System.Type, global::Mikodev.Binary.IConverterCreator> ConverterCreators {{ get; }} = global::System.Collections.Immutable.ImmutableDictionary.CreateRange(new global::System.Collections.Generic.Dictionary<global::System.Type, global::Mikodev.Binary.IConverterCreator>");
        builder.AppendIndent(1, $"{{");
        foreach (var content in targets)
        {
            cancellation.ThrowIfCancellationRequested();
            var fullName = Symbols.GetSymbolFullName(content.Symbol);
            builder.AppendIndent(2, $"{{ typeof({fullName}), new {content.ConverterCreatorTypeName}() }},");
            cancellation.ThrowIfCancellationRequested();
        }
        builder.AppendIndent(1, $"}});");
        foreach (var content in targets)
        {
            builder.AppendIndent();
            _ = builder.Append(content.Code);
            cancellation.ThrowIfCancellationRequested();
        }
        builder.AppendIndent(0, $"}}");

        var code = builder.ToString();
        context.AddSource(file, code);
    }
}
