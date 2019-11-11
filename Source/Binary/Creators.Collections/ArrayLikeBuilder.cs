using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using System;

namespace Mikodev.Binary.Creators.Collections
{
    internal abstract class ArrayLikeBuilder<T, E> : CollectionBuilder<T, ReadOnlyMemory<E>, MemoryItem<E>, E>
    {
        public override int Count(ReadOnlyMemory<E> item) => item.Length;
    }
}
