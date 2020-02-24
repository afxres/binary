using Mikodev.Binary.Internal.Adapters;
using System;

namespace Mikodev.Binary.Creators.Builders
{
    internal sealed class MemoryBuilder<T> : ArrayLikeBuilder<Memory<T>, T>
    {
        public override ReadOnlyMemory<T> Of(Memory<T> item) => item;

        public override Memory<T> To(CollectionAdapter<ArraySegment<T>> adapter, ReadOnlySpan<byte> span) => adapter.To(span);
    }
}
