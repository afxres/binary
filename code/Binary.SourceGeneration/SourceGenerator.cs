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
    private sealed class Entry
    {
        public Compilation Compilation { get; }

        public SourceProductionContext SourceProductionContext { get; }

        public string CurrentContainingNamespace { get; }

        public string CurrentTypeName { get; }

        public string CurrentFileName { get; }

        public ImmutableDictionary<ITypeSymbol, AttributeData> CurrentInclusions { get; }

        public Entry(Compilation compilation, SourceProductionContext sourceProductionContext, string currentContainingNamespace, string currentTypeName, string currentFileName, ImmutableDictionary<ITypeSymbol, AttributeData> currentInclusions)
        {
            Compilation = compilation;
            SourceProductionContext = sourceProductionContext;
            CurrentContainingNamespace = currentContainingNamespace;
            CurrentTypeName = currentTypeName;
            CurrentFileName = currentFileName;
            CurrentInclusions = currentInclusions;
        }
    }

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

    private static void Invoke(Compilation compilation, ImmutableArray<TypeDeclarationSyntax> declarations, SourceProductionContext production)
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
        var cancellation = production.CancellationToken;
        var include = compilation.GetTypeByMetadataName(Constants.SourceGeneratorIncludeAttributeTypeName)?.ConstructUnboundGenericType();

        var dictionary = new SortedDictionary<string, SortedDictionary<string, INamedTypeSymbol>>(StringComparer.InvariantCulture);
        void Insert(INamedTypeSymbol symbol)
        {
            var name = symbol.Name;
            var @namespace = symbol.ContainingNamespace.ToDisplayString();
            if (dictionary.TryGetValue(name, out var child) is false)
                dictionary.Add(name, child = new SortedDictionary<string, INamedTypeSymbol>(StringComparer.InvariantCulture));
            child.Add(@namespace, symbol);
        }

        foreach (var declaration in declarations)
        {
            var model = compilation.GetSemanticModel(declaration.SyntaxTree);
            var type = model.GetDeclaredSymbol(declaration);
            if (type is null)
                continue;
            if (Ensure(declaration, type) is { } descriptor)
                production.ReportDiagnostic(Diagnostic.Create(descriptor, Symbols.GetLocation(type), new object[] { Symbols.GetSymbolDiagnosticDisplay(type) }));
            else
                Insert(type);
            cancellation.ThrowIfCancellationRequested();
        }

        foreach (var i in dictionary)
        {
            var name = i.Key;
            var index = 0;
            cancellation.ThrowIfCancellationRequested();
            foreach (var pair in i.Value)
            {
                var file = $"{name}.{index}.g.cs";
                var @namespace = pair.Key;
                var inclusions = GetInclusions(production, pair.Value, include);
                var entry = new Entry(compilation, production, @namespace, name, file, inclusions);
                cancellation.ThrowIfCancellationRequested();
                Invoke(entry);
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

    private static void Invoke(Entry entry)
    {
        var inclusions = entry.CurrentInclusions;
        var production = entry.SourceProductionContext;
        var cancellation = production.CancellationToken;
        var pending = new Queue<ITypeSymbol>(inclusions.Keys);
        var handled = new SortedDictionary<string, SymbolConverterContent?>(StringComparer.InvariantCulture);
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

        Finish(entry, handled);
    }

    private static void Finish(Entry entry, SortedDictionary<string, SymbolConverterContent?> dictionary)
    {
        var context = entry.SourceProductionContext;
        var builder = new StringBuilder();
        var cancellation = context.CancellationToken;

        builder.AppendIndent(0, $"namespace {entry.CurrentContainingNamespace};");
        builder.AppendIndent();
        builder.AppendIndent(0, $"partial class {entry.CurrentTypeName}");
        builder.AppendIndent(0, $"{{");
        builder.AppendIndent(1, $"public static System.Collections.Generic.IReadOnlyDictionary<System.Type, Mikodev.Binary.IConverterCreator> ConverterCreators {{ get; }} = System.Collections.Immutable.ImmutableDictionary.CreateRange(new System.Collections.Generic.Dictionary<System.Type, Mikodev.Binary.IConverterCreator>");
        builder.AppendIndent(1, $"{{");
        foreach (var i in dictionary)
        {
            var content = i.Value;
            if (content is null)
                continue;
            cancellation.ThrowIfCancellationRequested();
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
            _ = builder.Append(content.Code);
            cancellation.ThrowIfCancellationRequested();
        }
        builder.AppendIndent(0, $"}}");

        var code = builder.ToString();
        var file = entry.CurrentFileName;
        context.AddSource(file, code);
    }
}
