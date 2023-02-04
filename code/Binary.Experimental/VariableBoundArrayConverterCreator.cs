namespace Mikodev.Binary.Experimental;

using System;
using System.Diagnostics.CodeAnalysis;

public class VariableBoundArrayConverterCreator : IConverterCreator
{
    [RequiresUnreferencedCode("Require unreferenced variable bound array types for binary serialization.")]
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        if (type.IsVariableBoundArray is false)
            return null;
        var itemType = type.GetElementType()!;
        var itemConverter = context.GetConverter(itemType);
        var converterType = typeof(VariableBoundArrayConverter<,>).MakeGenericType(type, itemType);
        var converter = Activator.CreateInstance(converterType, new object[] { itemConverter })!;
        return (IConverter)converter;
    }
}
