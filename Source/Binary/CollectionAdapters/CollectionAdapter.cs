using System;

namespace Mikodev.Binary.CollectionAdapters
{
    internal abstract class CollectionAdapter<U, E>
    {
        public abstract void Of(ref Allocator allocator, U item);

        public abstract ArraySegment<E> To(in ReadOnlySpan<byte> span);
    }
}
