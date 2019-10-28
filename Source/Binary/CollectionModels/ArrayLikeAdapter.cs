using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.CollectionModels
{
    internal abstract class ArrayLikeAdapter<T> : CollectionAdapter<ReadOnlyMemory<T>, MemoryItem<T>, T> { }
}
