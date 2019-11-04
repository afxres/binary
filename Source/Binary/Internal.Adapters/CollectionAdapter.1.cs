using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal abstract class CollectionAdapter<R>
    {
        public abstract R To(ReadOnlySpan<byte> span);
    }
}
