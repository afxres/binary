using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal abstract class CollectionBuilder<T, U, R, E> : CollectionBuilder
    {
        public abstract int Count(U item);

        public abstract U Of(T item);

        public abstract T To(CollectionAdapter<R> adapter, in ReadOnlySpan<byte> span);
    }
}
