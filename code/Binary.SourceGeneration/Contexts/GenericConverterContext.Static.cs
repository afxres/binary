namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

public sealed partial class GenericConverterContext
{
    private enum TypeArgumentsOption
    {
        None,

        IncludeReturnType,
    }

    private class TypeInfo(string name, TypeArgumentsOption option, ImmutableArray<ITypeSymbol> elements)
    {
        public string TypeName { get; } = name;

        public TypeArgumentsOption TypeArgumentsOption { get; } = option;

        public ImmutableArray<ITypeSymbol> ElementTypes { get; } = elements;
    }

    private static ImmutableHashSet<INamedTypeSymbol> CreateResource(Compilation compilation)
    {
        static void Register(Compilation compilation, ImmutableHashSet<INamedTypeSymbol>.Builder builder, string name)
        {
            if (compilation.GetTypeByMetadataName(name)?.ConstructUnboundGenericType() is not { } type)
                return;
            _ = builder.Add(type);
        }

        var builder = ImmutableHashSet.CreateBuilder<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        Register(compilation, builder, "System.ArraySegment`1");
        Register(compilation, builder, "System.Memory`1");
        Register(compilation, builder, "System.Nullable`1");
        Register(compilation, builder, "System.ReadOnlyMemory`1");
        Register(compilation, builder, "System.Buffers.ReadOnlySequence`1");
        Register(compilation, builder, "System.Collections.Generic.List`1");
        Register(compilation, builder, "System.Collections.Generic.Dictionary`2");
        Register(compilation, builder, "System.Collections.Generic.HashSet`1");
        Register(compilation, builder, "System.Collections.Generic.KeyValuePair`2");
        Register(compilation, builder, "System.Collections.Generic.LinkedList`1");
        Register(compilation, builder, "System.Collections.Generic.PriorityQueue`2");
        Register(compilation, builder, "System.Collections.Immutable.ImmutableArray`1");

        return builder.ToImmutable();
    }

    private static TypeInfo? GetInfo(SourceGeneratorContext context, ITypeSymbol type)
    {
        if (type.TypeKind is TypeKind.Enum)
            return new TypeInfo("Enum", TypeArgumentsOption.IncludeReturnType, ImmutableArray.Create<ITypeSymbol>());
        if (type is IArrayTypeSymbol array)
            return array.IsSZArray
                ? new TypeInfo("Array", TypeArgumentsOption.None, ImmutableArray.Create(array.ElementType))
                : new TypeInfo("VariableBoundArray", TypeArgumentsOption.IncludeReturnType, ImmutableArray.Create(array.ElementType));
        if (type is not INamedTypeSymbol symbol || symbol.IsGenericType is false)
            return null;
        const string ResourceKey = "Generic";
        var types = (ImmutableHashSet<INamedTypeSymbol>)context.GetOrCreateResource(ResourceKey, CreateResource);
        var unbound = symbol.ConstructUnboundGenericType();
        if (types.Contains(unbound))
            return new TypeInfo(symbol.Name, TypeArgumentsOption.None, symbol.TypeArguments);
        return null;
    }

    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        if (GetInfo(context, symbol) is not { } info)
            return null;
        return new GenericConverterContext(context, tracker, symbol, info).Invoke();
    }
}
