namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

public sealed partial class CollectionConverterContext
{
    private const string ConstructorArgument = "item";

    private enum ConstructorArgumentKind
    {
        Null,

        List,

        HashSet,

        Dictionary,

        ListKeyValuePair,
    }

    private class TypeBaseInfo(ConstructorArgumentKind kind, string expression)
    {
        public ConstructorArgumentKind ConstructorArgumentKind { get; } = kind;

        public string ConstructorExpression { get; } = expression;
    }

    private class TypeInfo(ConstructorArgumentKind kind, string expression, ImmutableArray<ITypeSymbol> types) : TypeBaseInfo(kind, expression)
    {
        public ImmutableArray<ITypeSymbol> ElementTypes { get; } = types;
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
        static void Register(Compilation compilation, ImmutableDictionary<INamedTypeSymbol, TypeBaseInfo>.Builder result, string name, ConstructorArgumentKind source, string method)
        {
            if (compilation.GetTypeByMetadataName(name) is not { } type)
                return;
            result.Add(type.ConstructUnboundGenericType(), new TypeBaseInfo(source, method));
        }

        var result = ImmutableDictionary.CreateBuilder<INamedTypeSymbol, TypeBaseInfo>(SymbolEqualityComparer.Default);

        Register(compilation, result, "System.Collections.Frozen.FrozenSet`1", ConstructorArgumentKind.List, $"System.Collections.Frozen.FrozenSet.ToFrozenSet({ConstructorArgument})");
        Register(compilation, result, "System.Collections.Frozen.FrozenDictionary`2", ConstructorArgumentKind.ListKeyValuePair, $"System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary({ConstructorArgument})");
        Register(compilation, result, "System.Collections.Generic.IList`1", ConstructorArgumentKind.List, ConstructorArgument);
        Register(compilation, result, "System.Collections.Generic.ICollection`1", ConstructorArgumentKind.List, ConstructorArgument);
        Register(compilation, result, "System.Collections.Generic.IEnumerable`1", ConstructorArgumentKind.List, ConstructorArgument);
        Register(compilation, result, "System.Collections.Generic.IReadOnlyList`1", ConstructorArgumentKind.List, ConstructorArgument);
        Register(compilation, result, "System.Collections.Generic.IReadOnlyCollection`1", ConstructorArgumentKind.List, ConstructorArgument);
        Register(compilation, result, "System.Collections.Generic.ISet`1", ConstructorArgumentKind.HashSet, ConstructorArgument);
        Register(compilation, result, "System.Collections.Generic.IReadOnlySet`1", ConstructorArgumentKind.HashSet, ConstructorArgument);
        Register(compilation, result, "System.Collections.Generic.IDictionary`2", ConstructorArgumentKind.Dictionary, ConstructorArgument);
        Register(compilation, result, "System.Collections.Generic.IReadOnlyDictionary`2", ConstructorArgumentKind.Dictionary, ConstructorArgument);
        Register(compilation, result, "System.Collections.Immutable.IImmutableDictionary`2", ConstructorArgumentKind.ListKeyValuePair, $"System.Collections.Immutable.ImmutableDictionary.CreateRange({ConstructorArgument})");
        Register(compilation, result, "System.Collections.Immutable.IImmutableList`1", ConstructorArgumentKind.List, $"System.Collections.Immutable.ImmutableList.CreateRange({ConstructorArgument})");
        Register(compilation, result, "System.Collections.Immutable.IImmutableQueue`1", ConstructorArgumentKind.List, $"System.Collections.Immutable.ImmutableQueue.CreateRange({ConstructorArgument})");
        Register(compilation, result, "System.Collections.Immutable.IImmutableSet`1", ConstructorArgumentKind.List, $"System.Collections.Immutable.ImmutableHashSet.CreateRange({ConstructorArgument})");
        Register(compilation, result, "System.Collections.Immutable.ImmutableDictionary`2", ConstructorArgumentKind.ListKeyValuePair, $"System.Collections.Immutable.ImmutableDictionary.CreateRange({ConstructorArgument})");
        Register(compilation, result, "System.Collections.Immutable.ImmutableHashSet`1", ConstructorArgumentKind.List, $"System.Collections.Immutable.ImmutableHashSet.CreateRange({ConstructorArgument})");
        Register(compilation, result, "System.Collections.Immutable.ImmutableList`1", ConstructorArgumentKind.List, $"System.Collections.Immutable.ImmutableList.CreateRange({ConstructorArgument})");
        Register(compilation, result, "System.Collections.Immutable.ImmutableQueue`1", ConstructorArgumentKind.List, $"System.Collections.Immutable.ImmutableQueue.CreateRange({ConstructorArgument})");
        Register(compilation, result, "System.Collections.Immutable.ImmutableSortedDictionary`2", ConstructorArgumentKind.ListKeyValuePair, $"System.Collections.Immutable.ImmutableSortedDictionary.CreateRange({ConstructorArgument})");
        Register(compilation, result, "System.Collections.Immutable.ImmutableSortedSet`1", ConstructorArgumentKind.List, $"System.Collections.Immutable.ImmutableSortedSet.CreateRange({ConstructorArgument})");

        return result.ToImmutable();
    }

    private static ImmutableHashSet<INamedTypeSymbol> CreateResourceForUnsupportedTypes(Compilation compilation)
    {
        static void Register(Compilation compilation, ImmutableHashSet<INamedTypeSymbol>.Builder result, string name)
        {
            if (compilation.GetTypeByMetadataName(name) is not { } type)
                return;
            _ = result.Add(type.IsGenericType ? type.ConstructUnboundGenericType() : type);
        }

        var result = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        Register(compilation, result, "System.String");
        Register(compilation, result, "System.Collections.Generic.Stack`1");
        Register(compilation, result, "System.Collections.Concurrent.ConcurrentStack`1");
        Register(compilation, result, "System.Collections.Immutable.ImmutableStack`1");
        Register(compilation, result, "System.Collections.Immutable.IImmutableStack`1");

        return result.ToImmutable();
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
            return new TypeInfo(ConstructorArgumentKind.Dictionary, string.Empty, dictionaryInterface.TypeArguments);
        var readonlyDictionaryInterface = interfaces.FirstOrDefault(x => Implements(x, resource.UnboundIReadOnlyDictionaryType));
        if (readonlyDictionaryInterface is not null && HasConstructor(symbol, readonlyDictionaryInterface))
            return new TypeInfo(ConstructorArgumentKind.Dictionary, string.Empty, readonlyDictionaryInterface.TypeArguments);
        var enumerableInterface = enumerableInterfaces.Single();
        var typeArguments = dictionaryInterface?.TypeArguments ?? readonlyDictionaryInterface?.TypeArguments;
        var hasConstructor = HasConstructor(symbol, enumerableInterface);
        var constructorArgumentKind = hasConstructor
            ? (typeArguments is null ? ConstructorArgumentKind.List : ConstructorArgumentKind.ListKeyValuePair)
            : ConstructorArgumentKind.Null;
        return new TypeInfo(constructorArgumentKind, string.Empty, typeArguments ?? enumerableInterface.TypeArguments);
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
            return new TypeInfo(definition.ConstructorArgumentKind, definition.ConstructorExpression, symbol.TypeArguments);
        return GetInfo(symbol, resource);
    }

    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        if (GetInfo(context, symbol) is not { } info)
            return null;
        return new CollectionConverterContext(context, tracker, symbol, info).Invoke();
    }
}
