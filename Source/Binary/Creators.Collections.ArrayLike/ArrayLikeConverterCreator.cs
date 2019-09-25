using Mikodev.Binary.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Collections.ArrayLike
{
    internal sealed class ArrayLikeConverterCreator : IConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(Memory<>)] = typeof(MemoryConverter<>),
            [typeof(ReadOnlyMemory<>)] = typeof(ReadOnlyMemoryConverter<>),
            [typeof(ArraySegment<>)] = typeof(ArraySegmentConverter<>),
        };

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsGenericType || !dictionary.TryGetValue(type.GetGenericTypeDefinition(), out var converterDefinition))
                return null;
            var itemType = type.GetGenericArguments().Single();
            var adapter = AdapterHelper.Create(context.GetConverter(itemType));
            var converter = Activator.CreateInstance(converterDefinition.MakeGenericType(itemType), adapter);
            return (Converter)converter;
        }
    }
}
