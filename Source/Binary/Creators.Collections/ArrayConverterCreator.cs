using System;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ArrayConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsArray)
                return null;
            if (type.GetArrayRank() != 1)
                throw new NotSupportedException("Multidimensional arrays are not supported, use array of arrays instead.");
            var itemType = type.GetElementType();
            var converterArguments = new object[] { context.GetConverter(itemType) };
            var converter = Activator.CreateInstance(typeof(ArrayConverter<>).MakeGenericType(itemType), converterArguments);
            return (Converter)converter;
        }
    }
}
