namespace Mikodev.Binary.Experimental;

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

public class MultidimensionalArrayConverterCreator : IConverterCreator
{
    private static readonly ImmutableDictionary<int, Type> ConverterDefinitions;

    static MultidimensionalArrayConverterCreator()
    {
        var builder = ImmutableDictionary.CreateBuilder<int, Type>();
        builder.Add(2, typeof(Array2DConverter<>));
        builder.Add(3, typeof(Array3DConverter<>));
        builder.Add(4, typeof(Array4DConverter<>));
        ConverterDefinitions = builder.ToImmutable();
    }

    [RequiresUnreferencedCode("Require unreferenced multidimensional array types for binary serialization.")]
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        static IConverter? Invoke(IGeneratorContext context, Type converterDefinition, Type itemType)
        {
            var itemConverter = context.GetConverter(itemType);
            var converterType = converterDefinition.MakeGenericType(itemType);
            var converter = Activator.CreateInstance(converterType, itemConverter);
            return (IConverter)converter!;
        }

        if (type.IsArray is false || ConverterDefinitions.TryGetValue(type.GetArrayRank(), out var definition) is false)
            return null;
        return Invoke(context, definition, type.GetElementType()!);
    }
}
