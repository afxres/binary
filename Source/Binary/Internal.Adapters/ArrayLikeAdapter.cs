using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal abstract class ArrayLikeAdapter<T> : CollectionAdapter<ReadOnlyMemory<T>, ArraySegment<T>>
    {
        public sealed override int Count(ReadOnlyMemory<T> item) => item.Length;
    }
}
