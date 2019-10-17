using Mikodev.Binary.CollectionAdapters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArrayLikeConverterCreator : IConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(ArraySegment<>)] = typeof(ArraySegmentCollectionConvert<>),
            [typeof(Memory<>)] = typeof(MemoryCollectionConvert<>),
            [typeof(ReadOnlyMemory<>)] = typeof(ReadOnlyMemoryCollectionConvert<>),
        };

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsGenericType || !dictionary.TryGetValue(type.GetGenericTypeDefinition(), out var definition))
                return null;
            var itemType = type.GetGenericArguments().Single();
            var itemConverter = context.GetConverter(itemType);
            var adapter = CollectionAdapterHelper.Create(itemConverter);
            var converterType = typeof(CollectionAdaptedConverter<,,>).MakeGenericType(type, typeof(ReadOnlyMemory<>).MakeGenericType(itemType), itemType);
            var converterArguments = new object[] { itemConverter, adapter, Activator.CreateInstance(definition.MakeGenericType(itemType)) };
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
