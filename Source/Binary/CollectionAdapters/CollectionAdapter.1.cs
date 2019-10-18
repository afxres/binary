using System;

namespace Mikodev.Binary.CollectionAdapters
{
    internal abstract class CollectionAdapter<E>
    {
        public abstract ArraySegment<E> To(in ReadOnlySpan<byte> span);
    }
}
