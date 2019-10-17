using System;

namespace Mikodev.Binary.CollectionAdapters
{
    internal abstract class CollectionConvert<T, U, E> : CollectionConvert
    {
        public abstract int Length(U item);

        public abstract U Of(T item);

        public abstract T To(in ArraySegment<E> item);
    }
}
