namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Converters.Endianness;
using System;
using System.Collections.Immutable;
using System.Collections.Specialized;

internal static class FallbackEndiannessMethods
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
    });

    internal static IConverter GetConverter(Type type)
    {
        static IConverter Invoke(Type type, bool native)
        {
            if (Types.Contains(type) is false && type.IsEnum is false)
                return null;
            var definition = native
                ? typeof(NativeEndianConverter<>)
                : typeof(LittleEndianConverter<>);
            var converterType = definition.MakeGenericType(type);
            var converter = Activator.CreateInstance(converterType);
            return (IConverter)converter;
        }

        return Invoke(type, BitConverter.IsLittleEndian);
    }
}
