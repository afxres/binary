namespace Mikodev.Binary.Creators.Endianness;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics.CodeAnalysis;

[RequiresDynamicCode(CommonDefine.RequiresDynamicCodeMessage)]
[RequiresUnreferencedCode(CommonDefine.RequiresUnreferencedCodeMessage)]
internal sealed class LittleEndianEnumConverterCreator : IConverterCreator
{
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        if (type.IsEnum is false)
            return null;
        var converterType = typeof(LittleEndianConverter<>).MakeGenericType(type);
        var converter = CommonModule.CreateInstance(converterType, null);
        return (IConverter)converter;
    }
}
