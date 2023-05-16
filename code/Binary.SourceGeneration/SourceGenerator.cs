namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mikodev.Binary.SourceGeneration.Contexts;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

[Generator]
public sealed class SourceGenerator : IIncrementalGenerator
{
    private delegate SourceResult? TypeHandler(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol);

    private sealed class ContextInfo
    {
        public string NameInSourceCode { get; }

        public string NamespaceInSourceCode { get; }

        public ImmutableDictionary<ITypeSymbol, AttributeData> Inclusions { get; }

        public ContextInfo(INamedTypeSymbol symbol, ImmutableDictionary<ITypeSymbol, AttributeData> inclusions)
        {
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
        var context = new SourceGeneratorContext(compilation, production.ReportDiagnostic, cancellation);
        var dictionary = new SortedDictionary<string, SortedDictionary<string, ContextInfo>>();

        foreach (var declaration in declarations)
        {
            cancellation.ThrowIfCancellationRequested();
            var semantic = compilation.GetSemanticModel(declaration.SyntaxTree);
            var symbol = semantic.GetDeclaredSymbol(declaration);
            if (symbol is null)
                continue;
            if (Symbols.ValidateContextType(context, declaration, symbol) is false)
                continue;
            var inclusions = GetInclusions(context, symbol, include);
            var info = new ContextInfo(symbol, inclusions);
            // order by type name, then by containing namespace
            if (dictionary.TryGetValue(symbol.Name, out var child) is false)
                dictionary.Add(symbol.Name, child = new SortedDictionary<string, ContextInfo>());
            child.Add(symbol.ContainingNamespace.ToDisplayString(), info);
        }

        foreach (var i in dictionary)
        {
            var name = i.Key;
            var child = i.Value;
            var index = 0;
            cancellation.ThrowIfCancellationRequested();
            foreach (var k in child)
            {
                var info = k.Value;
                cancellation.ThrowIfCancellationRequested();
                var code = Invoke(context, info);
                var file = $"{name}.{index}.g.cs";
                production.AddSource(file, code);
                index++;
            }
        }
    }

    private static ImmutableDictionary<ITypeSymbol, AttributeData> GetInclusions(SourceGeneratorContext context, INamedTypeSymbol type, INamedTypeSymbol? include)
    {
        var builder = ImmutableDictionary.CreateBuilder<ITypeSymbol, AttributeData>(SymbolEqualityComparer.Default);
        var attributes = type.GetAttributes();
        var cancellation = context.CancellationToken;
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
            if (Symbols.ValidateIncludeType(context, builder, attribute, includedType) is false)
                continue;
            builder.Add(includedType, attribute);
        }
        return builder.ToImmutable();
    }

    private static string Invoke(SourceGeneratorContext context, ContextInfo info)
    {
        var inclusions = info.Inclusions;
        var cancellation = context.CancellationToken;
        var pending = new Queue<ITypeSymbol>(inclusions.Keys);
        var tracker = new SourceGeneratorTracker(pending);
        var handled = new SortedDictionary<string, SourceResult?>();

        while (pending.Count is not 0)
        {
            var symbol = pending.Dequeue();
            var key = context.GetTypeFullName(symbol);
            cancellation.ThrowIfCancellationRequested();
            if (handled.ContainsKey(key))
                continue;
            var result = Handle(context, tracker, info, symbol);
            handled.Add(key, result);
        }
        return Finish(info, handled, cancellation);
    }

    private static SourceResult? Handle(SourceGeneratorContext context, SourceGeneratorTracker tracker, ContextInfo info, ITypeSymbol symbol)
    {
        if (context.ValidateType(symbol) is false)
            return null;

        var inclusions = info.Inclusions;
        var cancellation = context.CancellationToken;
        var result = default(SourceResult);

        foreach (var handler in TypeHandlers)
        {
            result = handler.Invoke(context, tracker, symbol);
            cancellation.ThrowIfCancellationRequested();
            if (result is null)
                continue;
            if (result.Status is SourceStatus.Ok)
                return result;
            if (result.Diagnostic is not { } diagnostic)
                break;
            context.Collect(diagnostic);
            return null;
        }

        if (inclusions.TryGetValue(symbol, out var attribute) is false)
            return null;
        var descriptor = result?.Status is SourceStatus.NoAvailableMember
            ? Constants.NoAvailableMemberFound
            : Constants.NoConverterGenerated;
        context.Collect(Diagnostic.Create(descriptor, Symbols.GetLocation(attribute), new object[] { Symbols.GetSymbolDiagnosticDisplayString(symbol) }));
        return null;
    }

    private static string Finish(ContextInfo info, SortedDictionary<string, SourceResult?> dictionary, CancellationToken cancellation)
    {
        var builder = new StringBuilder();
        builder.AppendIndent(0, $"namespace {info.NamespaceInSourceCode};");
        builder.AppendIndent();
        builder.AppendIndent(0, $"partial class {info.NameInSourceCode}");
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
