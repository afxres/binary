using Mikodev.Binary.Adapters;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Collections.ArrayLike
{
    internal sealed class ArrayLikeConverterCreator : IConverterCreator
    {
        private static readonly GenericTypeMatcher matcher = new GenericTypeMatcher(new Dictionary<Type, Type>
        {
            [typeof(Memory<>)] = typeof(MemoryConverter<>),
            [typeof(ReadOnlyMemory<>)] = typeof(ReadOnlyMemoryConverter<>),
            [typeof(ArraySegment<>)] = typeof(ArraySegmentConverter<>),
        });

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!matcher.Match(type, out var converterDefinition))
                return null;
            var itemType = type.GetGenericArguments().Single();
            var adapter = AdapterHelper.Create(context.GetConverter(itemType));
            var converter = Activator.CreateInstance(converterDefinition.MakeGenericType(itemType), adapter);
            return (Converter)converter;
        }
    }
}
