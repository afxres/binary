using System;

namespace Mikodev.Binary.CollectionAdapters
{
    internal abstract class CollectionAdapter<R, E>
    {
        public abstract R To(in ReadOnlySpan<byte> span);
    }
}
