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

        public INamedTypeSymbol Symbol { get; }

        public string NameInSourceCode { get; }

        public string NamespaceInSourceCode { get; }

        public ImmutableDictionary<ITypeSymbol, AttributeData> Inclusions { get; }

        public Entry(Compilation compilation, SourceProductionContext production, INamedTypeSymbol symbol, ImmutableDictionary<ITypeSymbol, AttributeData> inclusions)
        {
            Compilation = compilation;
            SourceProductionContext = production;
            Symbol = symbol;
            NameInSourceCode = Symbols.GetNameInSourceCode(symbol.Name);
            NamespaceInSourceCode = Symbols.GetNamespaceInSourceCode(symbol.ContainingNamespace);
            Inclusions = inclusions;
        }
    }

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
            (syntax, _) => (TypeDeclarationSyntax)syntax.TargetNode);
        var provider = context.CompilationProvider.Combine(declarations.Collect());
        context.RegisterSourceOutput(provider, (production, source) => Invoke(source.Left, production, source.Right));
    }

    private static void Invoke(Compilation compilation, SourceProductionContext production, ImmutableArray<TypeDeclarationSyntax> declarations)
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

        static void Insert(SortedDictionary<string, SortedDictionary<string, Entry>> dictionary, Entry entry)
        {
            // order by type name, then by containing namespace
            var symbol = entry.Symbol;
            if (dictionary.TryGetValue(symbol.Name, out var child) is false)
                dictionary.Add(symbol.Name, child = new SortedDictionary<string, Entry>());
            child.Add(symbol.ContainingNamespace.ToDisplayString(), entry);
        }

        if (declarations.IsDefaultOrEmpty)
            return;
        var cancellation = production.CancellationToken;
        var include = compilation.GetTypeByMetadataName(Constants.SourceGeneratorIncludeAttributeTypeName)?.ConstructUnboundGenericType();
        var dictionary = new SortedDictionary<string, SortedDictionary<string, Entry>>();

        foreach (var declaration in declarations)
        {
            var model = compilation.GetSemanticModel(declaration.SyntaxTree);
            var type = model.GetDeclaredSymbol(declaration);
            if (type is null)
                continue;
            if (Ensure(declaration, type) is { } descriptor)
                production.ReportDiagnostic(Diagnostic.Create(descriptor, Symbols.GetLocation(type), new object[] { Symbols.GetSymbolDiagnosticDisplay(type) }));
            else
                Insert(dictionary, new Entry(compilation, production, type, GetInclusions(production, type, include)));
            cancellation.ThrowIfCancellationRequested();
        }

        foreach (var i in dictionary)
        {
            var name = i.Key;
            var index = 0;
            cancellation.ThrowIfCancellationRequested();
            foreach (var pair in i.Value)
            {
                var entry = pair.Value;
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
                production.ReportDiagnostic(Diagnostic.Create(Constants.RequireSupportedTypeForIncludeAttribute, Symbols.GetLocation(i), new object[] { Symbols.GetSymbolDiagnosticDisplay(includedType) }));
            else if (builder.ContainsKey(includedType))
                production.ReportDiagnostic(Diagnostic.Create(Constants.IncludeTypeDuplicated, Symbols.GetLocation(i), new object[] { Symbols.GetSymbolDiagnosticDisplay(includedType) }));
            else
                builder.Add(includedType, i);
            cancellation.ThrowIfCancellationRequested();
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

        SymbolConverterContent? Handle(ITypeSymbol symbol)
        {
            if (Symbols.Validate(context, symbol, production) is false)
                return null;
            var result = TypeHandlers.Select(h => h.Invoke(context, symbol)).FirstOrDefault(x => x is not null);
            if (result is SymbolConverterContent target)
                return target;
            var diagnostic = ((Diagnostic?)result) ?? (inclusions.TryGetValue(symbol, out var attribute)
                ? Diagnostic.Create(Constants.NoConverterGenerated, Symbols.GetLocation(attribute), new object[] { Symbols.GetSymbolDiagnosticDisplay(symbol) })
                : null);
            if (diagnostic is not null)
                production.ReportDiagnostic(diagnostic);
            return null;
        }

        while (pending.Count is not 0)
        {
            cancellation.ThrowIfCancellationRequested();
            var symbol = pending.Dequeue();
            var key = context.GetTypeFullName(symbol);
            if (handled.ContainsKey(key))
                continue;
            var result = Handle(symbol);
            handled.Add(key, result);
            cancellation.ThrowIfCancellationRequested();
        }

        return Finish(entry, handled);
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
