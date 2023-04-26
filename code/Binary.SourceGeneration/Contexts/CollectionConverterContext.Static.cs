namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

public sealed partial class CollectionConverterContext
{
    private enum SourceType
    {
        Null,

        List,

        HashSet,

        Dictionary,

        ListKeyValuePair,
    }

    private class Resource
    {
        public INamedTypeSymbol? UnboundIEnumerableTypeSymbol { get; }

        public INamedTypeSymbol? UnboundIDictionaryTypeSymbol { get; }

        public INamedTypeSymbol? UnboundIReadOnlyDictionaryTypeSymbol { get; }

        public ImmutableDictionary<INamedTypeSymbol, (SourceType SourceType, string MethodBody)> SupportedTypeSymbols { get; }

        public ImmutableHashSet<INamedTypeSymbol> UnsupportedTypeSymbols { get; }

        public Resource(ImmutableDictionary<INamedTypeSymbol, (SourceType, string)> supported, ImmutableHashSet<INamedTypeSymbol> unsupported, INamedTypeSymbol? enumerable, INamedTypeSymbol? dictionary, INamedTypeSymbol? readonlyDictionary)
        {
            SupportedTypeSymbols = supported;
            UnsupportedTypeSymbols = unsupported;
            UnboundIEnumerableTypeSymbol = enumerable;
            UnboundIDictionaryTypeSymbol = dictionary;
            UnboundIReadOnlyDictionaryTypeSymbol = readonlyDictionary;
        }
    }

    private static Resource CreateResource(Compilation compilation)
    {
        var supportedBuilder = ImmutableDictionary.CreateBuilder<INamedTypeSymbol, (SourceType, string)>(SymbolEqualityComparer.Default);
        void Add(string name, SourceType source, string method)
        {
            if (compilation.GetTypeByMetadataName(name) is not { } type)
                return;
            supportedBuilder.Add(type.ConstructUnboundGenericType(), (source, method));
        }

        var unsupportedBuilder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        void AddUnsupported(string name)
        {
            if (compilation.GetTypeByMetadataName(name) is not { } type)
                return;
            _ = unsupportedBuilder.Add(type.IsGenericType ? type.ConstructUnboundGenericType() : type);
        }

        const string Lambda = "static x => x";
        Add("System.Collections.Frozen.FrozenSet`1", SourceType.List, "static x => System.Collections.Frozen.FrozenSet.ToFrozenSet(x, true)");
        Add("System.Collections.Frozen.FrozenDictionary`2", SourceType.ListKeyValuePair, "static x => System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(x, true)");
        Add("System.Collections.Generic.IList`1", SourceType.List, Lambda);
        Add("System.Collections.Generic.ICollection`1", SourceType.List, Lambda);
        Add("System.Collections.Generic.IEnumerable`1", SourceType.List, Lambda);
        Add("System.Collections.Generic.IReadOnlyList`1", SourceType.List, Lambda);
        Add("System.Collections.Generic.IReadOnlyCollection`1", SourceType.List, Lambda);
        Add("System.Collections.Generic.ISet`1", SourceType.HashSet, Lambda);
        Add("System.Collections.Generic.IReadOnlySet`1", SourceType.HashSet, Lambda);
        Add("System.Collections.Generic.IDictionary`2", SourceType.Dictionary, Lambda);
        Add("System.Collections.Generic.IReadOnlyDictionary`2", SourceType.Dictionary, Lambda);
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

        AddUnsupported("System.String");
        AddUnsupported("System.Collections.Generic.Stack`1");
        AddUnsupported("System.Collections.Concurrent.ConcurrentStack`1");
        AddUnsupported("System.Collections.Immutable.ImmutableStack`1");
        AddUnsupported("System.Collections.Immutable.IImmutableStack`1");

        var supported = supportedBuilder.ToImmutable();
        var unsupported = unsupportedBuilder.ToImmutable();
        var enumerable = supported.Keys.FirstOrDefault(x => x.Name is "IEnumerable");
        var dictionary = supported.Keys.FirstOrDefault(x => x.Name is "IDictionary");
        var readonlyDictionary = supported.Keys.FirstOrDefault(x => x.Name is "IReadOnlyDictionary");
        return new Resource(supported, unsupported, enumerable, dictionary, readonlyDictionary);
    }

    private static (SourceType SourceType, string MethodBody, ImmutableArray<ITypeSymbol> Elements)? GetInfo(INamedTypeSymbol symbol, Resource resource)
    {
        static bool Implements(INamedTypeSymbol current, INamedTypeSymbol? unboundTypeSymbol)
        {
            return current.IsGenericType && SymbolEqualityComparer.Default.Equals(unboundTypeSymbol, current.ConstructUnboundGenericType());
        }

        static bool HasConstructor(INamedTypeSymbol symbol, INamedTypeSymbol argument)
        {
            return Symbols.GetConstructor(symbol, argument) is not null;
        }

        var interfaces = symbol.AllInterfaces;
        var enumerableInterfaces = interfaces.Where(x => Implements(x, resource.UnboundIEnumerableTypeSymbol)).ToList();
        if (enumerableInterfaces.Count is not 1)
            return null;

        var dictionaryInterface = interfaces.FirstOrDefault(x => Implements(x, resource.UnboundIDictionaryTypeSymbol));
        if (dictionaryInterface is not null && HasConstructor(symbol, dictionaryInterface))
            return (SourceType.Dictionary, string.Empty, dictionaryInterface.TypeArguments);
        var readonlyDictionaryInterface = interfaces.FirstOrDefault(x => Implements(x, resource.UnboundIReadOnlyDictionaryTypeSymbol));
        if (readonlyDictionaryInterface is not null && HasConstructor(symbol, readonlyDictionaryInterface))
            return (SourceType.Dictionary, string.Empty, readonlyDictionaryInterface.TypeArguments);
        var enumerableInterface = enumerableInterfaces.Single();
        var typeArguments = dictionaryInterface?.TypeArguments ?? readonlyDictionaryInterface?.TypeArguments;
        var hasConstructor = HasConstructor(symbol, enumerableInterface);
        var sourceType = hasConstructor
            ? (typeArguments is null ? SourceType.List : SourceType.ListKeyValuePair)
            : SourceType.Null;
        return (sourceType, string.Empty, typeArguments ?? enumerableInterface.TypeArguments);
    }

    private static (SourceType SourceType, string MethodBody, ImmutableArray<ITypeSymbol> Elements)? GetInfo(SourceGeneratorContext context, ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol symbol)
            return null;
        const string ResourceKey = "Collections";
        if (context.Resources.TryGetValue(ResourceKey, out var result) is false)
            context.Resources.Add(ResourceKey, result = CreateResource(context.Compilation));
        var resource = (Resource)result;
        var unbound = symbol.IsGenericType ? symbol.ConstructUnboundGenericType() : null;
        var unboundOrOriginal = unbound ?? symbol;
        if (resource.UnsupportedTypeSymbols.Contains(unboundOrOriginal))
            return null;
        if (unbound is not null && resource.SupportedTypeSymbols.TryGetValue(unbound, out var definition) is true)
            return (definition.SourceType, definition.MethodBody, symbol.TypeArguments);
        return GetInfo(symbol, resource);
    }

    public static SymbolConverterContent? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (GetInfo(context, symbol) is not (var sourceName, var methodBody, var elements))
            return null;
        return new CollectionConverterContext(context, symbol, sourceName, methodBody, elements).Invoke();
    }
}
