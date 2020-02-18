using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal abstract class ArrayLikeAdapter<T> : CollectionAdapter<ReadOnlyMemory<T>, MemoryItem<T>> { }
}
