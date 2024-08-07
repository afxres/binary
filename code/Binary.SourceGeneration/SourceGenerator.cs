﻿namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mikodev.Binary.SourceGeneration.Contexts;
using Mikodev.Binary.SourceGeneration.Internal;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

[Generator]
public sealed class SourceGenerator : IIncrementalGenerator
{
    private delegate SourceResult? TypeHandler(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol);

    private sealed class ContextInfo(INamedTypeSymbol symbol, ImmutableDictionary<ITypeSymbol, AttributeData> inclusions)
    {
        public string NameInSourceCode { get; } = Symbols.GetNameInSourceCode(symbol.Name);

        public string NamespaceInSourceCode { get; } = Symbols.GetNamespaceInSourceCode(symbol.ContainingNamespace);

        public ImmutableDictionary<ITypeSymbol, AttributeData> Inclusions { get; } = inclusions;
    }

    private static readonly ImmutableArray<TypeHandler> CustomTypeHandlers =
    [
        AttributeConverterContext.Invoke,
        AttributeConverterCreatorContext.Invoke,
        TupleObjectConverterContext.Invoke,
        NamedObjectConverterContext.Invoke,
    ];

    private static readonly ImmutableArray<TypeHandler> NativeTypeHandlers =
    [
        GenericConverterContext.Invoke,
        CollectionConverterContext.Invoke,
        InlineArrayConverterContext.Invoke,
        TupleObjectConverterContext.Invoke,
        NamedObjectConverterContext.Invoke,
    ];

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
            var symbol = semantic.GetDeclaredSymbol(declaration, cancellation);
            if (symbol is null || Symbols.ValidateContextType(context, declaration, symbol) is false)
                continue;
            var inclusions = GetInclusions(context, symbol, include);
            var info = new ContextInfo(symbol, inclusions);
            // order by type name, then by containing namespace
            if (dictionary.TryGetValue(symbol.Name, out var child) is false)
                dictionary.Add(symbol.Name, child = []);
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
        var result = ImmutableDictionary.CreateBuilder<ITypeSymbol, AttributeData>(SymbolEqualityComparer.Default);
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
            if (Symbols.ValidateIncludeType(context, result, attribute, includedType) is false)
                continue;
            result.Add(includedType, attribute);
        }
        return result.ToImmutable();
    }

    private static string Invoke(SourceGeneratorContext context, ContextInfo info)
    {
        var inclusions = info.Inclusions;
        var cancellation = context.CancellationToken;
        var pending = new Queue<ITypeSymbol>(inclusions.Keys);
        var tracker = new SourceGeneratorTracker(pending.Enqueue);
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
        var kind = context.ValidateType(symbol);
        if (kind is SymbolTypeKind.BadType)
            return null;

        var inclusions = info.Inclusions;
        var cancellation = context.CancellationToken;
        var handlers = kind is SymbolTypeKind.RawType
            ? NativeTypeHandlers
            : CustomTypeHandlers;
        var result = handlers
            .Select(x => x.Invoke(context, tracker, symbol))
            .FirstOrDefault(x => x is not null);

        if (result?.Status is SourceStatus.Ok)
            return result;
        if (inclusions.TryGetValue(symbol, out var attribute) is false)
            return null;

        var symbolText = Symbols.GetSymbolDiagnosticDisplayString(symbol);
        if (result?.Status is SourceStatus.NoAvailableMember)
            if (context.GetTypeInfo(symbol).ConflictFieldsAndProperties is { Length: not 0 } targets)
                targets.ForEach(name => context.Collect(Constants.AmbiguousMemberFound.With(attribute, [name, symbolText])));
            else
                context.Collect(Constants.NoAvailableMemberFound.With(attribute, [symbolText]));
        else
            context.Collect(Constants.NoConverterGenerated.With(attribute, [symbolText]));
        return null;
    }

    private static string Finish(ContextInfo info, SortedDictionary<string, SourceResult?> dictionary, CancellationToken cancellation)
    {
        var output = new StringBuilder();
        output.AppendIndent(0, $"namespace {info.NamespaceInSourceCode};");
        output.AppendIndent();
        output.AppendIndent(0, $"partial class {info.NameInSourceCode}");
        output.AppendIndent(0, $"{{");
        output.AppendIndent(1, $"public static System.Collections.Generic.IReadOnlyDictionary<System.Type, Mikodev.Binary.IConverterCreator> ConverterCreators {{ get; }} = System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(new System.Collections.Generic.Dictionary<System.Type, Mikodev.Binary.IConverterCreator>");
        output.AppendIndent(1, $"{{");
        foreach (var i in dictionary)
        {
            var content = i.Value;
            if (content is null)
                continue;
            output.AppendIndent(2, $"{{ typeof({i.Key}), new {content.ConverterCreatorTypeName}() }},");
            cancellation.ThrowIfCancellationRequested();
        }
        output.AppendIndent(1, $"}});");
        foreach (var i in dictionary)
        {
            var content = i.Value;
            if (content is null)
                continue;
            output.AppendIndent();
            _ = output.Append(content.SourceCode);
            cancellation.ThrowIfCancellationRequested();
        }
        output.AppendIndent(0, $"}}");
        return output.ToString();
    }
}
