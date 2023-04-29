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

    private static (string Name, ImmutableArray<ITypeSymbol> Elements, SelfType SelfType)? GetInfo(SourceGeneratorContext context, ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array)
            return (array.IsSZArray ? "Array" : "VariableBoundArray", ImmutableArray.Create(array.ElementType), array.IsSZArray ? SelfType.Exclude : SelfType.Include);
        if (type is not INamedTypeSymbol symbol || symbol.IsGenericType is false)
            return null;
        const string ResourceKey = "Generic";
        var types = (ImmutableHashSet<INamedTypeSymbol>)context.GetOrCreateResource(ResourceKey, CreateResource);
        var unbound = symbol.ConstructUnboundGenericType();
        if (types.Contains(unbound))
            return (symbol.Name, symbol.TypeArguments, SelfType.Exclude);
        return null;
    }

    public static SymbolConverterContent? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (GetInfo(context, symbol) is not (var name, var elements, var self))
            return null;
        return new GenericConverterContext(context, symbol, name, elements, self).Invoke();
    }
}
