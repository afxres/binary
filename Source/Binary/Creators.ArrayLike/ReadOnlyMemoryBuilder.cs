using Mikodev.Binary.CollectionAdapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ReadOnlyMemoryBuilder<T> : CollectionBuilder<ReadOnlyMemory<T>, ReadOnlyMemory<T>, ArraySegment<T>, T>
    {
        public override int Length(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(ReadOnlyMemory<T> item) => item;

        public override ReadOnlyMemory<T> To(CollectionAdapter<ArraySegment<T>, T> adapter, in ReadOnlySpan<byte> span) => adapter.To(in span);
    }
}
