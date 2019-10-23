using Mikodev.Binary.CollectionModels;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IEnumerableBuilder<T, E> : CollectionBuilder<T, T, ArraySegment<E>, E> where T : IEnumerable<E>
    {
        public override int Count(T item) => item is ICollection<E> collection ? collection.Count : NoActualLength;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<ArraySegment<E>> adapter, in ReadOnlySpan<byte> span) => (T)(object)adapter.To(in span);
    }
}
