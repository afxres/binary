using Mikodev.Binary.Internal.Adapters;
using System;

namespace Mikodev.Binary.Creators.Builders
{
    internal sealed class ArrayConverterCreator : IConverterCreator
    {
        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsArray)
                return null;
            var itemType = type.GetElementType();
            if (type != itemType.MakeArrayType())
                throw new NotSupportedException($"Only single dimensional zero based arrays are supported, type: {type}");
            var itemConverter = context.GetConverter(itemType);
            var builder = Activator.CreateInstance(typeof(ArrayBuilder<>).MakeGenericType(itemType));
            var converterArguments = new object[] { itemConverter, builder };
            var converterType = typeof(ArrayLikeAdaptedConverter<,>).MakeGenericType(type, itemType);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
