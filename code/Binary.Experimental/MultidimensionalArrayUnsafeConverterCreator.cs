namespace Mikodev.Binary.Experimental;

using System;
using System.Diagnostics.CodeAnalysis;

public class MultidimensionalArrayUnsafeConverterCreator : IConverterCreator
{
    [RequiresUnreferencedCode("Require unreferenced multidimensional array types for binary serialization.")]
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        if (type.IsArray is false)
            return null;
        var itemType = type.GetElementType()!;
        var itemConverter = context.GetConverter(itemType);
        var converterType = typeof(MultidimensionalArrayUnsafeConverter<,>).MakeGenericType(type, itemType);
        var converter = Activator.CreateInstance(converterType, new object[] { itemConverter })!;
        return (IConverter)converter;
    }
}
