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

        public ImmutableDictionary<INamedTypeSymbol, (SourceType SourceType, string MethodBody)> Dictionary { get; }

        public Resource(ImmutableDictionary<INamedTypeSymbol, (SourceType, string)> dictionary, INamedTypeSymbol? unboundIEnumerableTypeSymbol, INamedTypeSymbol? unboundIDictionaryTypeSymbol, INamedTypeSymbol? unboundIReadOnlyDictionaryTypeSymbol)
        {
            Dictionary = dictionary;
            UnboundIEnumerableTypeSymbol = unboundIEnumerableTypeSymbol;
            UnboundIDictionaryTypeSymbol = unboundIDictionaryTypeSymbol;
            UnboundIReadOnlyDictionaryTypeSymbol = unboundIReadOnlyDictionaryTypeSymbol;
        }
    }

    private static Resource CreateResource(Compilation compilation)
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

        var dictionary = builder.ToImmutable();
        var unboundIEnumerableTypeSymbol = dictionary.Keys.First(x => x.Name is "IEnumerable");
        var unboundIDictionaryTypeSymbol = dictionary.Keys.First(x => x.Name is "IDictionary");
        var unboundIReadOnlyDictionaryTypeSymbol = dictionary.Keys.First(x => x.Name is "IReadOnlyDictionary");
        return new Resource(dictionary, unboundIEnumerableTypeSymbol, unboundIDictionaryTypeSymbol, unboundIReadOnlyDictionaryTypeSymbol);
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
        if (symbol.IsGenericType && resource.Dictionary.TryGetValue(symbol.ConstructUnboundGenericType(), out var definition) is true)
            return (definition.SourceType, definition.MethodBody, symbol.TypeArguments);
        return GetInfo(symbol, resource);
    }

    public static SymbolConverterContent? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (SymbolEqualityComparer.Default.Equals(symbol, context.Compilation.GetSpecialType(SpecialType.System_String)))
            return null;
        if (GetInfo(context, symbol) is not (var sourceName, var methodBody, var elements))
            return null;
        return new CollectionConverterContext(context, symbol, sourceName, methodBody, elements).Invoke();
    }
}
