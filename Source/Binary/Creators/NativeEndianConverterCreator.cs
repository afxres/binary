using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators
{
    internal sealed class NativeEndianConverterCreator : IConverterCreator
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
        };

        public IConverter GetConverter(IGeneratorContext context, Type type)
        {
            if (BitConverter.IsLittleEndian is false)
                return null;
            if (Types.Contains(type) is false && type.IsEnum is false)
                return null;
            var converterType = typeof(NativeEndianConverter<>).MakeGenericType(type);
            var converter = Activator.CreateInstance(converterType);
            return (IConverter)converter;
        }
    }
}
