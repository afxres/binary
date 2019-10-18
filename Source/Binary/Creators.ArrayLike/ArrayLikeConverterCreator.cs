using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArrayLikeConverterCreator : IConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(ArraySegment<>)] = typeof(ArraySegmentBuilder<>),
            [typeof(Memory<>)] = typeof(MemoryBuilder<>),
            [typeof(ReadOnlyMemory<>)] = typeof(ReadOnlyMemoryBuilder<>),
        };

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsGenericType || !dictionary.TryGetValue(type.GetGenericTypeDefinition(), out var definition))
                return null;
            var itemType = type.GetGenericArguments().Single();
            var itemConverter = context.GetConverter(itemType);
            var converterType = typeof(ArrayLikeConverter<,>).MakeGenericType(type, itemType);
            var converterArguments = new object[] { itemConverter, Activator.CreateInstance(definition.MakeGenericType(itemType)) };
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
