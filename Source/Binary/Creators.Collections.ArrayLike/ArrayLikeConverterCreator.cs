using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections.ArrayLike
{
    internal sealed class ArrayLikeConverterCreator : GenericConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> dictionary = new Dictionary<Type, Type>
        {
            [typeof(Memory<>)] = typeof(MemoryConverter<>),
            [typeof(ReadOnlyMemory<>)] = typeof(ReadOnlyMemoryConverter<>),
            [typeof(ArraySegment<>)] = typeof(ArraySegmentConverter<>),
        };

        public ArrayLikeConverterCreator() : base(dictionary) { }
    }
}
