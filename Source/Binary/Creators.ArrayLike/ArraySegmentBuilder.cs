using Mikodev.Binary.CollectionModels;
using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArraySegmentBuilder<T> : ArrayLikeBuilder<ArraySegment<T>, T>
    {
        public override int Count(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(ArraySegment<T> item) => item;

        public override ArraySegment<T> To(CollectionAdapter<MemoryItem<T>> adapter, in ReadOnlySpan<byte> span) => adapter.To(in span).AsArraySegment();
    }
}
