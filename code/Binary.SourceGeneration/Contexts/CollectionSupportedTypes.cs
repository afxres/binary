namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

public sealed class CollectionSupportedTypes
{
    public enum ConstructorKind
    {
        Null,
        List,
        HashSet,
        Dictionary,
        KeyValueEnumerable,
    }

    public class Definition(ConstructorKind kind, string expression)
    {
        public ConstructorKind Kind { get; } = kind;

        public string Expression { get; } = expression;
    }

    public sealed class Info(ConstructorKind kind, string expression, ImmutableArray<ITypeSymbol> types) : Definition(kind, expression)
    {
        public ImmutableArray<ITypeSymbol> ElementTypes { get; } = types;
    }

    public const string ConstructorArgument = "item";

    private readonly ImmutableDictionary<INamedTypeSymbol, Definition> supported;

    private readonly ImmutableHashSet<INamedTypeSymbol> unsupported;

    private readonly INamedTypeSymbol? unboundIEnumerable;

    private readonly INamedTypeSymbol? unboundIDictionary;

    private readonly INamedTypeSymbol? unboundIReadOnlyDictionary;

    public int SupportedCount => this.supported.Count;

    public int UnsupportedCount => this.unsupported.Count;

    private CollectionSupportedTypes(ImmutableDictionary<INamedTypeSymbol, Definition> supported, ImmutableHashSet<INamedTypeSymbol> unsupported, INamedTypeSymbol? enumerable, INamedTypeSymbol? dictionary, INamedTypeSymbol? readonlyDictionary)
    {
        this.supported = supported;
        this.unsupported = unsupported;
        this.unboundIEnumerable = enumerable;
        this.unboundIDictionary = dictionary;
        this.unboundIReadOnlyDictionary = readonlyDictionary;
    }

    public static CollectionSupportedTypes Create(Compilation compilation)
    {
        static void RegisterSupportedType(Compilation compilation, ImmutableDictionary<INamedTypeSymbol, Definition>.Builder result, string name, ConstructorKind kind, string expression)
        {
            if (compilation.GetTypeByMetadataName(name) is not { } type)
                return;
            result.Add(type.ConstructUnboundGenericType(), new Definition(kind, expression));
        }

        static void RegisterUnsupportedType(Compilation compilation, ImmutableHashSet<INamedTypeSymbol>.Builder result, string name)
        {
            if (compilation.GetTypeByMetadataName(name) is not { } type)
                return;
            _ = result.Add(type.IsGenericType ? type.ConstructUnboundGenericType() : type);
        }

        var supported = ImmutableDictionary.CreateBuilder<INamedTypeSymbol, Definition>(SymbolEqualityComparer.Default);
        var unsupported = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        RegisterSupportedType(compilation, supported, "System.Collections.Frozen.FrozenSet`1", ConstructorKind.List, $"System.Collections.Frozen.FrozenSet.ToFrozenSet({ConstructorArgument})");
        RegisterSupportedType(compilation, supported, "System.Collections.Frozen.FrozenDictionary`2", ConstructorKind.KeyValueEnumerable, $"System.Collections.Frozen.FrozenDictionary.ToFrozenDictionary({ConstructorArgument})");
        RegisterSupportedType(compilation, supported, "System.Collections.Generic.IList`1", ConstructorKind.List, ConstructorArgument);
        RegisterSupportedType(compilation, supported, "System.Collections.Generic.ICollection`1", ConstructorKind.List, ConstructorArgument);
        RegisterSupportedType(compilation, supported, "System.Collections.Generic.IEnumerable`1", ConstructorKind.List, ConstructorArgument);
        RegisterSupportedType(compilation, supported, "System.Collections.Generic.IReadOnlyList`1", ConstructorKind.List, ConstructorArgument);
        RegisterSupportedType(compilation, supported, "System.Collections.Generic.IReadOnlyCollection`1", ConstructorKind.List, ConstructorArgument);
        RegisterSupportedType(compilation, supported, "System.Collections.Generic.ISet`1", ConstructorKind.HashSet, ConstructorArgument);
        RegisterSupportedType(compilation, supported, "System.Collections.Generic.IReadOnlySet`1", ConstructorKind.HashSet, ConstructorArgument);
        RegisterSupportedType(compilation, supported, "System.Collections.Generic.IDictionary`2", ConstructorKind.Dictionary, ConstructorArgument);
        RegisterSupportedType(compilation, supported, "System.Collections.Generic.IReadOnlyDictionary`2", ConstructorKind.Dictionary, ConstructorArgument);
        RegisterSupportedType(compilation, supported, "System.Collections.Immutable.IImmutableDictionary`2", ConstructorKind.KeyValueEnumerable, $"System.Collections.Immutable.ImmutableDictionary.CreateRange({ConstructorArgument})");
        RegisterSupportedType(compilation, supported, "System.Collections.Immutable.IImmutableList`1", ConstructorKind.List, $"System.Collections.Immutable.ImmutableList.CreateRange({ConstructorArgument})");
        RegisterSupportedType(compilation, supported, "System.Collections.Immutable.IImmutableQueue`1", ConstructorKind.List, $"System.Collections.Immutable.ImmutableQueue.CreateRange({ConstructorArgument})");
        RegisterSupportedType(compilation, supported, "System.Collections.Immutable.IImmutableSet`1", ConstructorKind.List, $"System.Collections.Immutable.ImmutableHashSet.CreateRange({ConstructorArgument})");
        RegisterSupportedType(compilation, supported, "System.Collections.Immutable.ImmutableDictionary`2", ConstructorKind.KeyValueEnumerable, $"System.Collections.Immutable.ImmutableDictionary.CreateRange({ConstructorArgument})");
        RegisterSupportedType(compilation, supported, "System.Collections.Immutable.ImmutableHashSet`1", ConstructorKind.List, $"System.Collections.Immutable.ImmutableHashSet.CreateRange({ConstructorArgument})");
        RegisterSupportedType(compilation, supported, "System.Collections.Immutable.ImmutableList`1", ConstructorKind.List, $"System.Collections.Immutable.ImmutableList.CreateRange({ConstructorArgument})");
        RegisterSupportedType(compilation, supported, "System.Collections.Immutable.ImmutableQueue`1", ConstructorKind.List, $"System.Collections.Immutable.ImmutableQueue.CreateRange({ConstructorArgument})");
        RegisterSupportedType(compilation, supported, "System.Collections.Immutable.ImmutableSortedDictionary`2", ConstructorKind.KeyValueEnumerable, $"System.Collections.Immutable.ImmutableSortedDictionary.CreateRange({ConstructorArgument})");
        RegisterSupportedType(compilation, supported, "System.Collections.Immutable.ImmutableSortedSet`1", ConstructorKind.List, $"System.Collections.Immutable.ImmutableSortedSet.CreateRange({ConstructorArgument})");

        RegisterUnsupportedType(compilation, unsupported, "System.String");
        RegisterUnsupportedType(compilation, unsupported, "System.Collections.Generic.Stack`1");
        RegisterUnsupportedType(compilation, unsupported, "System.Collections.Concurrent.ConcurrentStack`1");
        RegisterUnsupportedType(compilation, unsupported, "System.Collections.Immutable.ImmutableStack`1");
        RegisterUnsupportedType(compilation, unsupported, "System.Collections.Immutable.IImmutableStack`1");

        var enumerable = supported.Keys.FirstOrDefault(x => x.Name is "IEnumerable");
        var dictionary = supported.Keys.FirstOrDefault(x => x.Name is "IDictionary");
        var readonlyDictionary = supported.Keys.FirstOrDefault(x => x.Name is "IReadOnlyDictionary");
        return new CollectionSupportedTypes(supported.ToImmutable(), unsupported.ToImmutable(), enumerable, dictionary, readonlyDictionary);
    }

    public bool IsUnsupported(INamedTypeSymbol symbol)
    {
        var unboundOrOriginal = symbol.IsGenericType ? symbol.ConstructUnboundGenericType() : symbol;
        return this.unsupported.Contains(unboundOrOriginal);
    }

    public Info? GetInfo(INamedTypeSymbol symbol)
    {
        var unbound = symbol.IsGenericType ? symbol.ConstructUnboundGenericType() : null;
        return unbound is not null && this.supported.TryGetValue(unbound, out var definition)
            ? new Info(definition.Kind, definition.Expression, symbol.TypeArguments)
            : Probe(symbol);
    }

    private Info? Probe(INamedTypeSymbol symbol)
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
        var enumerableInterfaces = interfaces.Where(x => Implements(x, this.unboundIEnumerable)).ToList();
        if (enumerableInterfaces.Count is not 1)
            return null;
        var dictionaryInterface = interfaces.FirstOrDefault(x => Implements(x, this.unboundIDictionary));
        if (dictionaryInterface is not null && HasConstructor(symbol, dictionaryInterface))
            return new Info(ConstructorKind.Dictionary, string.Empty, dictionaryInterface.TypeArguments);
        var readonlyDictionaryInterface = interfaces.FirstOrDefault(x => Implements(x, this.unboundIReadOnlyDictionary));
        if (readonlyDictionaryInterface is not null && HasConstructor(symbol, readonlyDictionaryInterface))
            return new Info(ConstructorKind.Dictionary, string.Empty, readonlyDictionaryInterface.TypeArguments);
        var enumerableInterface = enumerableInterfaces.Single();
        var typeArguments = dictionaryInterface?.TypeArguments ?? readonlyDictionaryInterface?.TypeArguments;
        var hasConstructor = HasConstructor(symbol, enumerableInterface);
        var constructorArgumentKind = hasConstructor
            ? (typeArguments is null ? ConstructorKind.List : ConstructorKind.KeyValueEnumerable)
            : ConstructorKind.Null;
        return new Info(constructorArgumentKind, string.Empty, typeArguments ?? enumerableInterface.TypeArguments);
    }
}
