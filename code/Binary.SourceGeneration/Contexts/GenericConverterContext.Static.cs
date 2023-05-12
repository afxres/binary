namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

public sealed partial class GenericConverterContext
{
    private enum SelfType
    {
        Exclude,

        Include,
    }

    private class TypeInfo
    {
        public string Name { get; }

        public SelfType SelfType { get; }

        public ImmutableArray<ITypeSymbol> ElementTypes { get; }

        public TypeInfo(string name, SelfType selfType, ImmutableArray<ITypeSymbol> elements)
        {
            Name = name;
            SelfType = selfType;
            ElementTypes = elements;
        }
    }

    private static ImmutableHashSet<INamedTypeSymbol> CreateResource(Compilation compilation)
    {
        var builder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        void Add(string name)
        {
            if (compilation.GetTypeByMetadataName(name)?.ConstructUnboundGenericType() is not { } type)
                return;
            _ = builder.Add(type);
        }

        Add("System.ArraySegment`1");
        Add("System.Memory`1");
        Add("System.Nullable`1");
        Add("System.ReadOnlyMemory`1");
        Add("System.Buffers.ReadOnlySequence`1");
        Add("System.Collections.Generic.List`1");
        Add("System.Collections.Generic.Dictionary`2");
        Add("System.Collections.Generic.HashSet`1");
        Add("System.Collections.Generic.KeyValuePair`2");
        Add("System.Collections.Generic.LinkedList`1");
        Add("System.Collections.Generic.PriorityQueue`2");
        Add("System.Collections.Immutable.ImmutableArray`1");
        return builder.ToImmutable();
    }

    private static TypeInfo? GetInfo(SourceGeneratorContext context, ITypeSymbol type)
    {
        if (type.TypeKind is TypeKind.Enum)
            return new TypeInfo("Enum", SelfType.Include, ImmutableArray.Create<ITypeSymbol>());
        if (type is IArrayTypeSymbol array)
            return new TypeInfo(array.IsSZArray ? "Array" : "VariableBoundArray", array.IsSZArray ? SelfType.Exclude : SelfType.Include, ImmutableArray.Create(array.ElementType));
        if (type is not INamedTypeSymbol symbol || symbol.IsGenericType is false)
            return null;
        const string ResourceKey = "Generic";
        var types = (ImmutableHashSet<INamedTypeSymbol>)context.GetOrCreateResource(ResourceKey, CreateResource);
        var unbound = symbol.ConstructUnboundGenericType();
        if (types.Contains(unbound))
            return new TypeInfo(symbol.Name, SelfType.Exclude, symbol.TypeArguments);
        return null;
    }

    public static SymbolConverterContent? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (GetInfo(context, symbol) is not { } info)
            return null;
        return new GenericConverterContext(context, symbol, info).Invoke();
    }
}
