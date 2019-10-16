using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Endianness
{
    internal sealed class UnknownEndiannessConverterCreator : IConverterCreator
    {
        private static readonly IReadOnlyList<Type> types = new[]
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
            if (!types.Contains(type) && !type.IsEnum)
                return null;
            var converter = Activator.CreateInstance(typeof(CurrentEndiannessConverter<>).MakeGenericType(type));
            return (Converter)converter;
        }
    }
}
