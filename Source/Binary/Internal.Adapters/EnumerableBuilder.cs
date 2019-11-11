using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Adapters
{
    internal abstract class EnumerableBuilder<T, E> : CollectionBuilder<T, T, MemoryItem<E>, E>
    {
        public override int Count(T item) => item is ICollection<E> collection ? collection.Count : NoActualLength;

        public override T Of(T item) => item;
    }
}
