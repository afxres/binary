using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Builders
{
    internal sealed class ReadOnlyMemoryBuilder<T> : SpanLikeBuilder<ReadOnlyMemory<T>, T>
    {
        public override ReadOnlySpan<T> Handle(ReadOnlyMemory<T> item) => item.Span;

        public override ReadOnlyMemory<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var result = adapter.Decode(span);
            Debug.Assert((uint)result.Length <= (uint)result.Memory.Length);
            return new ReadOnlyMemory<T>(result.Memory, 0, result.Length);
        }
    }
}
