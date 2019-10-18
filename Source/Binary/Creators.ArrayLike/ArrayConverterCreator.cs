using System;

namespace Mikodev.Binary.Creators.ArrayLike
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
            var itemConverter = context.GetConverter(itemType);
            var converterArguments = new object[] { itemConverter };
            var converterType = typeof(ArrayConverter<>).MakeGenericType(itemType);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
