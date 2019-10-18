using System;

namespace Mikodev.Binary.CollectionAdapters
{
    internal abstract class CollectionBuilder<T, U, E> : CollectionBuilder
    {
        public abstract int Length(U item);

        public abstract U Of(T item);

        public abstract T To(CollectionAdapter<E> adapter, in ReadOnlySpan<byte> span);
    }
}
