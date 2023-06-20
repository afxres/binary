namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

public sealed partial class CollectionConverterContext
{
    private const string ConstructorParameter = "item";

    private enum SourceKind
    {
        Null,

        List,

        HashSet,

        Dictionary,

        ListKeyValuePair,
    }

    private class TypeBaseInfo(SourceKind sourceKind, string expression)
    {
        public SourceKind SourceKind { get; } = sourceKind;

        public string Expression { get; } = expression;
    }

    private class TypeInfo(SourceKind sourceKind, string expression, ImmutableArray<ITypeSymbol> elements) : TypeBaseInfo(sourceKind, expression)
    {
        public ImmutableArray<ITypeSymbol> ElementTypes { get; } = elements;
    }

    private class Resource(ImmutableDictionary<INamedTypeSymbol, TypeBaseInfo> supported, ImmutableHashSet<INamedTypeSymbol> unsupported, INamedTypeSymbol? enumerable, INamedTypeSymbol? dictionary, INamedTypeSymbol? readonlyDictionary)
    {
        public INamedTypeSymbol? UnboundIEnumerableType { get; } = enumerable;

        public INamedTypeSymbol? UnboundIDictionaryType { get; } = dictionary;

        public INamedTypeSymbol? UnboundIReadOnlyDictionaryType { get; } = readonlyDictionary;

        public ImmutableDictionary<INamedTypeSymbol, TypeBaseInfo> SupportedTypes { get; } = supported;

        public ImmutableHashSet<INamedTypeSymbol> UnsupportedTypes { get; } = unsupported;
    }

    private static ImmutableDictionary<INamedTypeSymbol, TypeBaseInfo> CreateResourceForSupportedTypes(Compilation compilation)
    {
        static void Add(Compilation compilation, ImmutableDictionary<INamedTypeSymbol, TypeBaseInfo>.Builder builder, string name, SourceKind source, string method)
        {
            if (compilation.GetTypeByMetadataName(name) is not { } type)
                return;
            builder.Add(type.ConstructUnboundGenericType(), new TypeBaseInfo(source, method));
        }

        var builder = ImmutableDictionary.CreateBuilder<INamedTypeSymbol, TypeBaseInfo>(SymbolEqualityComparer.Default);
        var functor = (string name, SourceKind source, string method) => Add(compilation, builder, name, source, method);
        functor.Invoke("System.Collections.Frozen.FrozenSet`1", SourceKind.List, $"System.Collections.Frozen.FrozenSet.ToFrozenSet({ConstructorParameter}, true)");
        functor.Invoke("System.Collections.Frozen.FrozenDictionary`2", SourceKind.ListKeyValuePair, $"System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary({ConstructorParameter}, true)");
        functor.Invoke("System.Collections.Generic.IList`1", SourceKind.List, ConstructorParameter);
        functor.Invoke("System.Collections.Generic.ICollection`1", SourceKind.List, ConstructorParameter);
        functor.Invoke("System.Collections.Generic.IEnumerable`1", SourceKind.List, ConstructorParameter);
        functor.Invoke("System.Collections.Generic.IReadOnlyList`1", SourceKind.List, ConstructorParameter);
        functor.Invoke("System.Collections.Generic.IReadOnlyCollection`1", SourceKind.List, ConstructorParameter);
        functor.Invoke("System.Collections.Generic.ISet`1", SourceKind.HashSet, ConstructorParameter);
        functor.Invoke("System.Collections.Generic.IReadOnlySet`1", SourceKind.HashSet, ConstructorParameter);
        functor.Invoke("System.Collections.Generic.IDictionary`2", SourceKind.Dictionary, ConstructorParameter);
        functor.Invoke("System.Collections.Generic.IReadOnlyDictionary`2", SourceKind.Dictionary, ConstructorParameter);
        functor.Invoke("System.Collections.Immutable.IImmutableDictionary`2", SourceKind.ListKeyValuePair, $"System.Collections.Immutable.ImmutableDictionary.CreateRange({ConstructorParameter})");
        functor.Invoke("System.Collections.Immutable.IImmutableList`1", SourceKind.List, $"System.Collections.Immutable.ImmutableList.CreateRange({ConstructorParameter})");
        functor.Invoke("System.Collections.Immutable.IImmutableQueue`1", SourceKind.List, $"System.Collections.Immutable.ImmutableQueue.CreateRange({ConstructorParameter})");
        functor.Invoke("System.Collections.Immutable.IImmutableSet`1", SourceKind.List, $"System.Collections.Immutable.ImmutableHashSet.CreateRange({ConstructorParameter})");
        functor.Invoke("System.Collections.Immutable.ImmutableDictionary`2", SourceKind.ListKeyValuePair, $"System.Collections.Immutable.ImmutableDictionary.CreateRange({ConstructorParameter})");
        functor.Invoke("System.Collections.Immutable.ImmutableHashSet`1", SourceKind.List, $"System.Collections.Immutable.ImmutableHashSet.CreateRange({ConstructorParameter})");
        functor.Invoke("System.Collections.Immutable.ImmutableList`1", SourceKind.List, $"System.Collections.Immutable.ImmutableList.CreateRange({ConstructorParameter})");
        functor.Invoke("System.Collections.Immutable.ImmutableQueue`1", SourceKind.List, $"System.Collections.Immutable.ImmutableQueue.CreateRange({ConstructorParameter})");
        functor.Invoke("System.Collections.Immutable.ImmutableSortedDictionary`2", SourceKind.ListKeyValuePair, $"System.Collections.Immutable.ImmutableSortedDictionary.CreateRange({ConstructorParameter})");
        functor.Invoke("System.Collections.Immutable.ImmutableSortedSet`1", SourceKind.List, $"System.Collections.Immutable.ImmutableSortedSet.CreateRange({ConstructorParameter})");
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
            return new TypeInfo(SourceKind.Dictionary, string.Empty, dictionaryInterface.TypeArguments);
        var readonlyDictionaryInterface = interfaces.FirstOrDefault(x => Implements(x, resource.UnboundIReadOnlyDictionaryType));
        if (readonlyDictionaryInterface is not null && HasConstructor(symbol, readonlyDictionaryInterface))
            return new TypeInfo(SourceKind.Dictionary, string.Empty, readonlyDictionaryInterface.TypeArguments);
        var enumerableInterface = enumerableInterfaces.Single();
        var typeArguments = dictionaryInterface?.TypeArguments ?? readonlyDictionaryInterface?.TypeArguments;
        var hasConstructor = HasConstructor(symbol, enumerableInterface);
        var sourceKind = hasConstructor
            ? (typeArguments is null ? SourceKind.List : SourceKind.ListKeyValuePair)
            : SourceKind.Null;
        return new TypeInfo(sourceKind, string.Empty, typeArguments ?? enumerableInterface.TypeArguments);
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
            return new TypeInfo(definition.SourceKind, definition.Expression, symbol.TypeArguments);
        return GetInfo(symbol, resource);
    }

    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        if (GetInfo(context, symbol) is not { } info)
            return null;
        return new CollectionConverterContext(context, tracker, symbol, info).Invoke();
    }
}
