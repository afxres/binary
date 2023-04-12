namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

public partial class CollectionConverterContext
{
    private enum SourceType
    {
        List,

        HashSet,

        Dictionary,

        ListKeyValuePair,
    }

    private static ImmutableDictionary<INamedTypeSymbol, (SourceType, string)> CreateResource(Compilation compilation)
    {
        var builder = ImmutableDictionary.CreateBuilder<INamedTypeSymbol, (SourceType, string)>(SymbolEqualityComparer.Default);
        void Add(string name, SourceType source, string method)
        {
            if (compilation.GetTypeByMetadataName(name)?.ConstructUnboundGenericType() is not { } type)
                return;
            builder.Add(type, (source, method));
        }

        Add("System.Collections.Generic.IList`1", SourceType.List, Constants.LambdaIdFunction);
        Add("System.Collections.Generic.ICollection`1", SourceType.List, Constants.LambdaIdFunction);
        Add("System.Collections.Generic.IEnumerable`1", SourceType.List, Constants.LambdaIdFunction);
        Add("System.Collections.Generic.IReadOnlyList`1", SourceType.List, Constants.LambdaIdFunction);
        Add("System.Collections.Generic.IReadOnlyCollection`1", SourceType.List, Constants.LambdaIdFunction);
        Add("System.Collections.Generic.ISet`1", SourceType.HashSet, Constants.LambdaIdFunction);
        Add("System.Collections.Generic.IReadOnlySet`1", SourceType.HashSet, Constants.LambdaIdFunction);
        Add("System.Collections.Generic.IDictionary`2", SourceType.Dictionary, Constants.LambdaIdFunction);
        Add("System.Collections.Generic.IReadOnlyDictionary`2", SourceType.Dictionary, Constants.LambdaIdFunction);
        Add("System.Collections.Immutable.IImmutableDictionary`2", SourceType.ListKeyValuePair, "System.Collections.Immutable.ImmutableDictionary.CreateRange");
        Add("System.Collections.Immutable.IImmutableList`1", SourceType.List, "System.Collections.Immutable.ImmutableList.CreateRange");
        Add("System.Collections.Immutable.IImmutableQueue`1", SourceType.List, "System.Collections.Immutable.ImmutableQueue.CreateRange");
        Add("System.Collections.Immutable.IImmutableSet`1", SourceType.List, "System.Collections.Immutable.ImmutableHashSet.CreateRange");
        Add("System.Collections.Immutable.ImmutableDictionary`2", SourceType.ListKeyValuePair, "System.Collections.Immutable.ImmutableDictionary.CreateRange");
        Add("System.Collections.Immutable.ImmutableHashSet`1", SourceType.List, "System.Collections.Immutable.ImmutableHashSet.CreateRange");
        Add("System.Collections.Immutable.ImmutableList`1", SourceType.List, "System.Collections.Immutable.ImmutableList.CreateRange");
        Add("System.Collections.Immutable.ImmutableQueue`1", SourceType.List, "System.Collections.Immutable.ImmutableQueue.CreateRange");
        Add("System.Collections.Immutable.ImmutableSortedDictionary`2", SourceType.ListKeyValuePair, "System.Collections.Immutable.ImmutableSortedDictionary.CreateRange");
        Add("System.Collections.Immutable.ImmutableSortedSet`1", SourceType.List, "System.Collections.Immutable.ImmutableSortedSet.CreateRange");
        return builder.ToImmutable();
    }

    private static (SourceType SourceType, string MethodBody, ImmutableArray<ITypeSymbol> Elements)? GetInfo(SourceGeneratorContext context, ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol symbol || symbol.IsGenericType is false)
            return null;
        const string ResourceKey = "Collections";
        if (context.Resources.TryGetValue(ResourceKey, out var result) is false)
            context.Resources.Add(ResourceKey, result = CreateResource(context.Compilation));
        var unbound = symbol.ConstructUnboundGenericType();
        if (((ImmutableDictionary<INamedTypeSymbol, (SourceType SourceType, string MethodBody)>)result).TryGetValue(unbound, out var definition) is true)
            return (definition.SourceType, definition.MethodBody, symbol.TypeArguments);
        return null;
    }

    public static SymbolConverterContent? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (GetInfo(context, symbol) is not (var sourceName, var methodBody, var elements))
            return null;
        return new CollectionConverterContext(context, symbol, sourceName, methodBody, elements).Invoke();
    }
}
