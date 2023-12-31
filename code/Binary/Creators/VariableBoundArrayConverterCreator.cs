namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics.CodeAnalysis;

[RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
internal sealed class VariableBoundArrayConverterCreator : IConverterCreator
{
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        if (type.IsVariableBoundArray is false || type.GetElementType() is not { } itemType)
            return null;
        var itemConverter = context.GetConverter(itemType);
        var converterType = typeof(VariableBoundArrayConverter<,>).MakeGenericType(type, itemType);
        var converter = CommonModule.CreateInstance(converterType, [itemConverter]);
        return (IConverter)converter;
    }
}
