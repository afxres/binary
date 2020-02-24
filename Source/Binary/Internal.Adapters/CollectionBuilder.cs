using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal abstract class CollectionBuilder<T, U, R>
    {
        public abstract U Of(T item);

        public abstract T To(CollectionAdapter<R> adapter, ReadOnlySpan<byte> span);
    }
}
