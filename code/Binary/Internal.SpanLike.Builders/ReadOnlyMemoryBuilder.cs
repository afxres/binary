using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Builders
{
    internal sealed class ReadOnlyMemoryBuilder<T> : SpanLikeBuilder<ReadOnlyMemory<T>, T>
    {
        public override ReadOnlySpan<T> Handle(ReadOnlyMemory<T> item) => item.Span;

        public override ReadOnlyMemory<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var (buffer, length) = adapter.Decode(span);
            Debug.Assert((uint)length <= (uint)buffer.Length);
            return new ReadOnlyMemory<T>(buffer, 0, length);
        }
    }
}
