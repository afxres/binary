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
            var itemConverter = context.GetConverter(itemType);
            var builder = Activator.CreateInstance(typeof(ArrayBuilder<>).MakeGenericType(itemType));
            var converterArguments = new object[] { itemConverter, builder };
            var converterType = typeof(ArrayLikeConverter<,>).MakeGenericType(type, itemType);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
