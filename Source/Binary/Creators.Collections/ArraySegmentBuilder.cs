using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using System;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ArraySegmentBuilder<T> : ArrayLikeBuilder<ArraySegment<T>, T>
    {
        public override int Count(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(ArraySegment<T> item) => item;

        public override ArraySegment<T> To(CollectionAdapter<MemoryItem<T>> adapter, ReadOnlySpan<byte> span) => adapter.To(span).AsArraySegment();
    }
}
