using Mikodev.Binary.CollectionModels;
using Mikodev.Binary.CollectionModels.Implementations;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IEnumerableBuilder<T, E> : EnumerableBuilder<T, E> where T : IEnumerable<E>
    {
        public override int Count(T item) => item is ICollection<E> collection ? collection.Count : NoActualLength;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<MemoryItem<E>> adapter, in ReadOnlySpan<byte> span) => (T)(object)adapter.To(in span).AsArraySegment();
    }
}
