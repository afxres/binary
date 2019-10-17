using System;

namespace Mikodev.Binary.CollectionAdapters
{
    internal abstract class CollectionConvert<T, E>
    {
        public abstract ReadOnlySpan<E> Of(T item);

        public abstract T To(in ArraySegment<E> item);
    }
}
