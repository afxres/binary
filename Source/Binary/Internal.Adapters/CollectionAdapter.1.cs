using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal abstract class CollectionAdapter<R>
    {
        public abstract R To(in ReadOnlySpan<byte> span);
    }
}
