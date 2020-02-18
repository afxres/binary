using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators
{
    internal sealed class OriginalEndiannessConverterCreator : IConverterCreator
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

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (BitConverter.IsLittleEndian != Converter.UseLittleEndian)
                throw new NotSupportedException("Endianness not supported!");
            if (!Types.Contains(type) && !type.IsEnum)
                return null;
            var converterType = typeof(OriginalEndiannessConverter<>).MakeGenericType(type);
            var converter = Activator.CreateInstance(converterType);
            return (Converter)converter;
        }
    }
}
