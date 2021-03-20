using Mikodev.Binary.Converters.Endianness;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackEndiannessMethods
    {
        private static readonly IReadOnlyCollection<Type> Types = new[]
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
            typeof(Guid),
            typeof(BitVector32),
#if NET5_0_OR_GREATER
            typeof(Half),
#endif
        };

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
}
