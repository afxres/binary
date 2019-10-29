using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal abstract class ArrayLikeBuilder<T, E> : CollectionBuilder<T, ReadOnlyMemory<E>, MemoryItem<E>, E> { }
}
