namespace Mikodev.Binary.Creators.Endianness;

using Mikodev.Binary.Internal;
using System;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

internal sealed class DetectEndianConverterCreator : IConverterCreator
{
    private static readonly ImmutableArray<Type> Types = ImmutableArray.Create(new[]
    {
        typeof(bool),
        typeof(byte),
        typeof(sbyte),
        typeof(char),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(Half),
        typeof(BitVector32),
        typeof(Int128),
        typeof(UInt128),
    });

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        static IConverter? Invoke(Type type, bool native)
        {
            if (Types.Contains(type) is false && type.IsEnum is false)
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
