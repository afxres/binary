namespace Mikodev.Binary.Creators.Endianness;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics.CodeAnalysis;

[RequiresUnreferencedCode(CommonDefine.RequiresUnreferencedCodeMessage)]
internal sealed class DetectEndianEnumConverterCreator : IConverterCreator
{
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        static IConverter? Invoke(Type type, bool native)
        {
            if (type.IsEnum is false)
                return null;
            var definition = native
                ? typeof(NativeEndianConverter<>)
                : typeof(LittleEndianConverter<>);
            var converterType = definition.MakeGenericType(type);
            var converter = CommonModule.CreateInstance(converterType, null);
            return (IConverter)converter;
        }

        return Invoke(type, BitConverter.IsLittleEndian);
    }
}
