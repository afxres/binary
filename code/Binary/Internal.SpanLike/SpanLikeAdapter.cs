using System;

namespace Mikodev.Binary.Internal.SpanLike
{
    internal abstract class SpanLikeAdapter<T>
    {
        public abstract void Encode(ref Allocator allocator, ReadOnlySpan<T> item);

        public abstract MemoryBuffer<T> Decode(ReadOnlySpan<byte> span);
    }
}
