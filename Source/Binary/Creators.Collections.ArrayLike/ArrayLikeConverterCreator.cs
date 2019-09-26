using Mikodev.Binary.Adapters;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Collections.ArrayLike
{
    internal sealed class ArrayLikeConverterCreator : IConverterCreator
    {
        private static readonly SimpleConverterCreator creator = new SimpleConverterCreator(new Dictionary<Type, Type>
        {
            [typeof(Memory<>)] = typeof(MemoryConverter<>),
            [typeof(ReadOnlyMemory<>)] = typeof(ReadOnlyMemoryConverter<>),
            [typeof(ArraySegment<>)] = typeof(ArraySegmentConverter<>),
        });

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            return creator.GetConverter(context, type, x => new[] { AdapterHelper.Create(x.Single()) });
        }
    }
}
