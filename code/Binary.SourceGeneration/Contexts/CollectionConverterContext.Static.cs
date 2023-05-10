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

    private class TypeInfo
    {
        public SourceType SourceType { get; }

        public string MethodBody { get; }

        public ImmutableArray<ITypeSymbol> ElementTypes { get; }

        public TypeInfo(SourceType sourceType, string methodBody, ImmutableArray<ITypeSymbol> elements)
        {
            SourceType = sourceType;
            MethodBody = methodBody;
            ElementTypes = elements;
        }
    }

    private class Resource
    {
        public INamedTypeSymbol? UnboundIEnumerableType { get; }

        public INamedTypeSymbol? UnboundIDictionaryType { get; }

        public INamedTypeSymbol? UnboundIReadOnlyDictionaryType { get; }

        public ImmutableDictionary<INamedTypeSymbol, (SourceType SourceType, string MethodBody)> SupportedTypes { get; }

        public ImmutableHashSet<INamedTypeSymbol> UnsupportedTypes { get; }

        public Resource(ImmutableDictionary<INamedTypeSymbol, (SourceType, string)> supported, ImmutableHashSet<INamedTypeSymbol> unsupported, INamedTypeSymbol? enumerable, INamedTypeSymbol? dictionary, INamedTypeSymbol? readonlyDictionary)
        {
            SupportedTypes = supported;
            UnsupportedTypes = unsupported;
            UnboundIEnumerableType = enumerable;
            UnboundIDictionaryType = dictionary;
            UnboundIReadOnlyDictionaryType = readonlyDictionary;
        }
    }

    private static ImmutableDictionary<INamedTypeSymbol, (SourceType, string)> CreateResourceForSupportedTypes(Compilation compilation)
    {
        static void Add(Compilation compilation, ImmutableDictionary<INamedTypeSymbol, (SourceType, string)>.Builder builder, string name, SourceType source, string method)
        {
            if (compilation.GetTypeByMetadataName(name) is not { } type)
                return;
            builder.Add(type.ConstructUnboundGenericType(), (source, method));
        }

        const string Lambda = "static x => x";
        var builder = ImmutableDictionary.CreateBuilder<INamedTypeSymbol, (SourceType, string)>(SymbolEqualityComparer.Default);
        var functor = (string name, SourceType source, string method) => Add(compilation, builder, name, source, method);
        functor.Invoke("System.Collections.Frozen.FrozenSet`1", SourceType.List, "static x => System.Collections.Frozen.FrozenSet.ToFrozenSet(x, true)");
        functor.Invoke("System.Collections.Frozen.FrozenDictionary`2", SourceType.ListKeyValuePair, "static x => System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary(x, true)");
        functor.Invoke("System.Collections.Generic.IList`1", SourceType.List, Lambda);
        functor.Invoke("System.Collections.Generic.ICollection`1", SourceType.List, Lambda);
        functor.Invoke("System.Collections.Generic.IEnumerable`1", SourceType.List, Lambda);
        functor.Invoke("System.Collections.Generic.IReadOnlyList`1", SourceType.List, Lambda);
        functor.Invoke("System.Collections.Generic.IReadOnlyCollection`1", SourceType.List, Lambda);
        functor.Invoke("System.Collections.Generic.ISet`1", SourceType.HashSet, Lambda);
        functor.Invoke("System.Collections.Generic.IReadOnlySet`1", SourceType.HashSet, Lambda);
        functor.Invoke("System.Collections.Generic.IDictionary`2", SourceType.Dictionary, Lambda);
        functor.Invoke("System.Collections.Generic.IReadOnlyDictionary`2", SourceType.Dictionary, Lambda);
        functor.Invoke("System.Collections.Immutable.IImmutableDictionary`2", SourceType.ListKeyValuePair, "System.Collections.Immutable.ImmutableDictionary.CreateRange");
        functor.Invoke("System.Collections.Immutable.IImmutableList`1", SourceType.List, "System.Collections.Immutable.ImmutableList.CreateRange");
        functor.Invoke("System.Collections.Immutable.IImmutableQueue`1", SourceType.List, "System.Collections.Immutable.ImmutableQueue.CreateRange");
        functor.Invoke("System.Collections.Immutable.IImmutableSet`1", SourceType.List, "System.Collections.Immutable.ImmutableHashSet.CreateRange");
        functor.Invoke("System.Collections.Immutable.ImmutableDictionary`2", SourceType.ListKeyValuePair, "System.Collections.Immutable.ImmutableDictionary.CreateRange");
        functor.Invoke("System.Collections.Immutable.ImmutableHashSet`1", SourceType.List, "System.Collections.Immutable.ImmutableHashSet.CreateRange");
        functor.Invoke("System.Collections.Immutable.ImmutableList`1", SourceType.List, "System.Collections.Immutable.ImmutableList.CreateRange");
        functor.Invoke("System.Collections.Immutable.ImmutableQueue`1", SourceType.List, "System.Collections.Immutable.ImmutableQueue.CreateRange");
        functor.Invoke("System.Collections.Immutable.ImmutableSortedDictionary`2", SourceType.ListKeyValuePair, "System.Collections.Immutable.ImmutableSortedDictionary.CreateRange");
        functor.Invoke("System.Collections.Immutable.ImmutableSortedSet`1", SourceType.List, "System.Collections.Immutable.ImmutableSortedSet.CreateRange");
        return builder.ToImmutable();
    }

    private static ImmutableHashSet<INamedTypeSymbol> CreateResourceForUnsupportedTypes(Compilation compilation)
    {
        static void Add(Compilation compilation, ImmutableHashSet<INamedTypeSymbol>.Builder builder, string name)
        {
            if (compilation.GetTypeByMetadataName(name) is not { } type)
                return;
            _ = builder.Add(type.IsGenericType ? type.ConstructUnboundGenericType() : type);
        }

        var builder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var functor = (string name) => Add(compilation, builder, name);
        functor.Invoke("System.String");
        functor.Invoke("System.Collections.Generic.Stack`1");
        functor.Invoke("System.Collections.Concurrent.ConcurrentStack`1");
        functor.Invoke("System.Collections.Immutable.ImmutableStack`1");
        functor.Invoke("System.Collections.Immutable.IImmutableStack`1");
        return builder.ToImmutable();
    }

    private static Resource CreateResource(Compilation compilation)
    {
        var supported = CreateResourceForSupportedTypes(compilation);
        var unsupported = CreateResourceForUnsupportedTypes(compilation);
        var enumerable = supported.Keys.FirstOrDefault(x => x.Name is "IEnumerable");
        var dictionary = supported.Keys.FirstOrDefault(x => x.Name is "IDictionary");
        var readonlyDictionary = supported.Keys.FirstOrDefault(x => x.Name is "IReadOnlyDictionary");
        return new Resource(supported, unsupported, enumerable, dictionary, readonlyDictionary);
    }

    private static TypeInfo? GetInfo(INamedTypeSymbol symbol, Resource resource)
    {
        static bool Implements(INamedTypeSymbol symbol, INamedTypeSymbol? unboundTypeSymbol)
        {
            return symbol.IsGenericType && SymbolEqualityComparer.Default.Equals(unboundTypeSymbol, symbol.ConstructUnboundGenericType());
        }

        static bool HasConstructor(INamedTypeSymbol symbol, INamedTypeSymbol argument)
        {
            return Symbols.GetConstructor(symbol, argument) is not null;
        }

        var interfaces = symbol.AllInterfaces;
        var enumerableInterfaces = interfaces.Where(x => Implements(x, resource.UnboundIEnumerableType)).ToList();
        if (enumerableInterfaces.Count is not 1)
            return null;

        var dictionaryInterface = interfaces.FirstOrDefault(x => Implements(x, resource.UnboundIDictionaryType));
        if (dictionaryInterface is not null && HasConstructor(symbol, dictionaryInterface))
            return new TypeInfo(SourceType.Dictionary, string.Empty, dictionaryInterface.TypeArguments);
        var readonlyDictionaryInterface = interfaces.FirstOrDefault(x => Implements(x, resource.UnboundIReadOnlyDictionaryType));
        if (readonlyDictionaryInterface is not null && HasConstructor(symbol, readonlyDictionaryInterface))
            return new TypeInfo(SourceType.Dictionary, string.Empty, readonlyDictionaryInterface.TypeArguments);
        var enumerableInterface = enumerableInterfaces.Single();
        var typeArguments = dictionaryInterface?.TypeArguments ?? readonlyDictionaryInterface?.TypeArguments;
        var hasConstructor = HasConstructor(symbol, enumerableInterface);
        var sourceType = hasConstructor
            ? (typeArguments is null ? SourceType.List : SourceType.ListKeyValuePair)
            : SourceType.Null;
        return new TypeInfo(sourceType, string.Empty, typeArguments ?? enumerableInterface.TypeArguments);
    }

    private static TypeInfo? GetInfo(SourceGeneratorContext context, ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol symbol)
            return null;
        const string ResourceKey = "Collections";
        var resource = (Resource)context.GetOrCreateResource(ResourceKey, CreateResource);
        var unbound = symbol.IsGenericType ? symbol.ConstructUnboundGenericType() : null;
        var unboundOrOriginal = unbound ?? symbol;
        if (resource.UnsupportedTypes.Contains(unboundOrOriginal))
            return null;
        if (unbound is not null && resource.SupportedTypes.TryGetValue(unbound, out var definition))
            return new TypeInfo(definition.SourceType, definition.MethodBody, symbol.TypeArguments);
        return GetInfo(symbol, resource);
    }

    public static SymbolConverterContent? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (GetInfo(context, symbol) is not { } info)
            return null;
        return new CollectionConverterContext(context, symbol, info).Invoke();
    }
}
