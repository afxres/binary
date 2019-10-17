using System;

namespace Mikodev.Binary.CollectionAdapters
{
    internal abstract class CollectionAdapter<T>
    {
        public abstract void Of(ref Allocator allocator, in ReadOnlySpan<T> span);

        public abstract ArraySegment<T> To(in ReadOnlySpan<byte> span);
    }
}
