using Mikodev.Binary.CollectionAdapters;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ISetBuilder<T, E> : CollectionBuilder<T, T, E> where T : ISet<E>
    {
        public override int Length(T item) => item.Count;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<E> adapter, in ReadOnlySpan<byte> span) => (T)(object)new HashSet<E>(adapter.To(in span));
    }
}
