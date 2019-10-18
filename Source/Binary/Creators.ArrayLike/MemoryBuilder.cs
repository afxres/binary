using Mikodev.Binary.CollectionAdapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class MemoryBuilder<T> : CollectionBuilder<Memory<T>, ReadOnlyMemory<T>, T>
    {
        public override int Length(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(Memory<T> item) => item;

        public override Memory<T> To(CollectionAdapter<T> adapter, in ReadOnlySpan<byte> span) => adapter.To(in span);
    }
}
