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
    private static readonly IEnumerable<Func<SourceGeneratorContext, ITypeSymbol, string?>> TypeHandlers = new List<Func<SourceGeneratorContext, ITypeSymbol, string?>>
    {
        AttributeConverterContext.Invoke,
        AttributeConverterCreatorContext.Invoke,
        GenericConverterContext.Invoke,
        CollectionConverterContext.Invoke,
        TupleObjectConverterContext.Invoke,
        NamedObjectConverterContext.Invoke,
    };

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
        var include = compilation.GetTypeByMetadataName(Constants.SourceGeneratorIncludeAttributeTypeName)?.ConstructUnboundGenericType();
        if (include is null)
            return;

        var cancellation = context.CancellationToken;
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

    private static void Invoke(Compilation compilation, SourceProductionContext context, INamedTypeSymbol type, INamedTypeSymbol include)
    {
        var includedTypes = new Dictionary<ITypeSymbol, AttributeData>(SymbolEqualityComparer.Default);
        var attributes = type.GetAttributes();
        var cancellation = context.CancellationToken;
        foreach (var i in attributes)
        {
            var attribute = i.AttributeClass;
            if (attribute is null || attribute.IsGenericType is false)
                continue;
            var definitions = attribute.ConstructUnboundGenericType();
            if (SymbolEqualityComparer.Default.Equals(definitions, include) is false)
                continue;
            if (attribute.TypeArguments.Single() is not ITypeSymbol includedType)
                continue;
            if (includedTypes.ContainsKey(includedType) is false)
                includedTypes.Add(includedType, i);
            else
                context.ReportDiagnostic(Diagnostic.Create(Constants.IncludeTypeDuplicated, Symbols.GetLocation(i), new[] { includedType.Name }));
            cancellation.ThrowIfCancellationRequested();
        }

        var pending = new Queue<ITypeSymbol>(includedTypes.Keys);
        var handled = new Dictionary<ITypeSymbol, string?>(SymbolEqualityComparer.Default);
        var generator = new SourceGeneratorContext(type, compilation, context, pending);

        void Handle(ITypeSymbol symbol)
        {
            try
            {
                Symbols.ValidateType(generator, symbol);
                var creator = TypeHandlers.Select(h => h.Invoke(generator, symbol)).OfType<string>().FirstOrDefault();
                if (creator is null && includedTypes.TryGetValue(symbol, out var attribute) is true)
                    context.ReportDiagnostic(Diagnostic.Create(Constants.NoConverterGenerated, Symbols.GetLocation(attribute), new[] { symbol.Name }));
                handled.Add(symbol, creator);
                cancellation.ThrowIfCancellationRequested();
            }
            catch (SourceGeneratorException e)
            {
                if (e.Diagnostic is { } diagnostic)
                    context.ReportDiagnostic(diagnostic);
                handled.Add(symbol, null);
                cancellation.ThrowIfCancellationRequested();
            }
        }

        while (pending.Count is not 0)
        {
            var symbol = pending.Dequeue();
            if (handled.ContainsKey(symbol))
                continue;
            Handle(symbol);
            cancellation.ThrowIfCancellationRequested();
        }

        AppendConverterCreators(context, generator, handled);
    }

    private static void AppendConverterCreators(SourceProductionContext context, SourceGeneratorContext generation, IDictionary<ITypeSymbol, string?> creators)
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
