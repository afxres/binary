using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using System;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class MemoryBuilder<T> : ArrayLikeBuilder<Memory<T>, T>
    {
        public override int Count(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(Memory<T> item) => item;

        public override Memory<T> To(CollectionAdapter<MemoryItem<T>> adapter, in ReadOnlySpan<byte> span) => adapter.To(in span).AsArraySegment();
    }
}
