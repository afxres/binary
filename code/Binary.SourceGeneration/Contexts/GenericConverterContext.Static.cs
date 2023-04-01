namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public partial class GenericConverterContext
{
    private static ImmutableArray<INamedTypeSymbol> CreateResource(Compilation compilation)
    {
        var builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();
        void Add(string name)
        {
            if (compilation.GetTypeByMetadataName(name)?.ConstructUnboundGenericType() is not { } type)
                return;
            builder.Add(type);
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

    private static (string Name, ImmutableArray<ITypeSymbol> Elements)? GetInfo(SourceGeneratorContext context, ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array && array.IsSZArray)
            return ("Array", ImmutableArray.Create(array.ElementType));
        if (type is not INamedTypeSymbol symbol || symbol.IsGenericType is false)
            return null;
        const string TupleLikeResourceKey = "TupleLike";
        if (context.Resources.TryGetValue(TupleLikeResourceKey, out var types) is false)
            context.Resources.Add(TupleLikeResourceKey, types = CreateResource(context.Compilation));
        var unbounded = symbol.ConstructUnboundGenericType();
        if ((types as IEnumerable<ISymbol>)?.FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x, unbounded)) is { } definition)
            return (definition.Name, symbol.TypeArguments);
        return null;
    }

    public static string? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (GetInfo(context, symbol) is not (var name, var elements))
            return null;
        var closure = new GenericConverterContext(context, symbol, name, elements);
        closure.Invoke();
        return closure.ConverterCreatorTypeName;
    }
}
