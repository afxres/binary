using System;

namespace Mikodev.Binary.CollectionModels
{
    internal abstract class CollectionAdapter<R>
    {
        public abstract R To(in ReadOnlySpan<byte> span);
    }
}
