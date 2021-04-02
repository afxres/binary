using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Builders
{
    internal sealed class MemoryBuilder<T> : SpanLikeBuilder<Memory<T>, T>
    {
        public override ReadOnlySpan<T> Handle(Memory<T> item) => item.Span;

        public override Memory<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var result = adapter.Decode(span);
            Debug.Assert((uint)result.Length <= (uint)result.Memory.Length);
            return new Memory<T>(result.Memory, 0, result.Length);
        }
    }
}
