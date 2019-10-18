using Mikodev.Binary.CollectionAdapters;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IEnumerableBuilder<T, E> : CollectionBuilder<T, T, E> where T : IEnumerable<E>
    {
        public override int Length(T item) => item is ICollection<E> collection ? collection.Count : NoActualLength;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<E> adapter, in ReadOnlySpan<byte> span) => (T)(object)adapter.To(in span);
    }
}
