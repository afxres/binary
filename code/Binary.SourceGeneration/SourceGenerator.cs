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

    private sealed class Entry
    {
        public Compilation Compilation { get; }

        public SourceProductionContext SourceProductionContext { get; }

        public string NameInSourceCode { get; }

        public string NamespaceInSourceCode { get; }

        public ImmutableDictionary<ITypeSymbol, AttributeData> Inclusions { get; }

        public Entry(Compilation compilation, SourceProductionContext production, INamedTypeSymbol symbol, ImmutableDictionary<ITypeSymbol, AttributeData> inclusions)
        {
            Compilation = compilation;
            SourceProductionContext = production;
            NameInSourceCode = Symbols.GetNameInSourceCode(symbol.Name);
            NamespaceInSourceCode = Symbols.GetNamespaceInSourceCode(symbol.ContainingNamespace);
            Inclusions = inclusions;
        }
    }

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
            (syntax, _) => (TypeDeclarationSyntax)syntax.TargetNode);
        var provider = context.CompilationProvider.Combine(declarations.Collect());
        context.RegisterSourceOutput(provider, (production, source) => Invoke(source.Left, production, source.Right));
    }

    private static void Invoke(Compilation compilation, SourceProductionContext production, ImmutableArray<TypeDeclarationSyntax> declarations)
    {
        if (declarations.IsDefaultOrEmpty)
            return;
        var cancellation = production.CancellationToken;
        var include = compilation.GetTypeByMetadataName(Constants.SourceGeneratorIncludeAttributeTypeName)?.ConstructUnboundGenericType();
        var dictionary = new SortedDictionary<string, SortedDictionary<string, Entry>>();

        foreach (var declaration in declarations)
        {
            cancellation.ThrowIfCancellationRequested();
            var semantic = compilation.GetSemanticModel(declaration.SyntaxTree);
            var symbol = semantic.GetDeclaredSymbol(declaration);
            if (symbol is null)
                continue;
            if (Symbols.ValidateContextType(production, declaration, symbol) is false)
                continue;
            var inclusions = GetInclusions(production, symbol, include);
            var entry = new Entry(compilation, production, symbol, inclusions);
            // order by type name, then by containing namespace
            if (dictionary.TryGetValue(symbol.Name, out var child) is false)
                dictionary.Add(symbol.Name, child = new SortedDictionary<string, Entry>());
            child.Add(symbol.ContainingNamespace.ToDisplayString(), entry);
        }

        foreach (var i in dictionary)
        {
            var name = i.Key;
            var child = i.Value;
            var index = 0;
            cancellation.ThrowIfCancellationRequested();
            foreach (var k in child)
            {
                var entry = k.Value;
                cancellation.ThrowIfCancellationRequested();
                var code = Invoke(entry);
                var file = $"{name}.{index}.g.cs";
                production.AddSource(file, code);
                index++;
            }
        }
    }

    private static ImmutableDictionary<ITypeSymbol, AttributeData> GetInclusions(SourceProductionContext production, INamedTypeSymbol type, INamedTypeSymbol? include)
    {
        var builder = ImmutableDictionary.CreateBuilder<ITypeSymbol, AttributeData>(SymbolEqualityComparer.Default);
        var attributes = type.GetAttributes();
        var cancellation = production.CancellationToken;
        foreach (var attribute in attributes)
        {
            cancellation.ThrowIfCancellationRequested();
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is null || attributeClass.IsGenericType is false)
                continue;
            var definitions = attributeClass.ConstructUnboundGenericType();
            if (SymbolEqualityComparer.Default.Equals(definitions, include) is false)
                continue;
            var includedType = attributeClass.TypeArguments.Single();
            if (Symbols.ValidateIncludeType(production, builder, attribute, includedType) is false)
                continue;
            builder.Add(includedType, attribute);
        }
        return builder.ToImmutable();
    }

    private static string Invoke(Entry entry)
    {
        var inclusions = entry.Inclusions;
        var production = entry.SourceProductionContext;
        var cancellation = production.CancellationToken;
        var pending = new Queue<ITypeSymbol>(inclusions.Keys);
        var handled = new SortedDictionary<string, SymbolConverterContent?>();
        var context = new SourceGeneratorContext(entry.Compilation, pending, cancellation);

        while (pending.Count is not 0)
        {
            var symbol = pending.Dequeue();
            var key = context.GetTypeFullName(symbol);
            cancellation.ThrowIfCancellationRequested();
            if (handled.ContainsKey(key))
                continue;
            var result = Handle(entry, context, symbol);
            handled.Add(key, result);
        }
        return Finish(entry, handled);
    }

    private static SymbolConverterContent? Handle(Entry entry, SourceGeneratorContext context, ITypeSymbol symbol)
    {
        var inclusions = entry.Inclusions;
        var production = entry.SourceProductionContext;
        var cancellation = production.CancellationToken;
        if (Symbols.ValidateType(production, context, symbol) is false)
            return null;

        foreach (var handler in TypeHandlers)
        {
            cancellation.ThrowIfCancellationRequested();
            var result = handler.Invoke(context, symbol);
            if (result is SymbolConverterContent content)
                return content;
            if (result is null)
                continue;
            var diagnostic = (Diagnostic)result;
            production.ReportDiagnostic(diagnostic);
            return null;
        }

        if (inclusions.TryGetValue(symbol, out var attribute))
            production.ReportDiagnostic(Diagnostic.Create(Constants.NoConverterGenerated, Symbols.GetLocation(attribute), new object[] { Symbols.GetSymbolDiagnosticDisplayString(symbol) }));
        return null;
    }

    private static string Finish(Entry entry, SortedDictionary<string, SymbolConverterContent?> dictionary)
    {
        var builder = new StringBuilder();
        var production = entry.SourceProductionContext;
        var cancellation = production.CancellationToken;

        builder.AppendIndent(0, $"namespace {entry.NamespaceInSourceCode};");
        builder.AppendIndent();
        builder.AppendIndent(0, $"partial class {entry.NameInSourceCode}");
        builder.AppendIndent(0, $"{{");
        builder.AppendIndent(1, $"public static System.Collections.Generic.IReadOnlyDictionary<System.Type, Mikodev.Binary.IConverterCreator> ConverterCreators {{ get; }} = System.Collections.Immutable.ImmutableDictionary.CreateRange(new System.Collections.Generic.Dictionary<System.Type, Mikodev.Binary.IConverterCreator>");
        builder.AppendIndent(1, $"{{");
        foreach (var i in dictionary)
        {
            var content = i.Value;
            if (content is null)
                continue;
            builder.AppendIndent(2, $"{{ typeof({i.Key}), new {content.ConverterCreatorTypeName}() }},");
            cancellation.ThrowIfCancellationRequested();
        }
        builder.AppendIndent(1, $"}});");
        foreach (var i in dictionary)
        {
            var content = i.Value;
            if (content is null)
                continue;
            builder.AppendIndent();
            _ = builder.Append(content.SourceCode);
            cancellation.ThrowIfCancellationRequested();
        }
        builder.AppendIndent(0, $"}}");
        return builder.ToString();
    }
}
