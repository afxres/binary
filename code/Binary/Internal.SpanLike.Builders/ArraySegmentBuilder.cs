using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Builders
{
    internal sealed class ArraySegmentBuilder<T> : SpanLikeBuilder<ArraySegment<T>, T>
    {
        public override ReadOnlySpan<T> Handle(ArraySegment<T> item) => item;

        public override ArraySegment<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var (buffer, length) = adapter.Decode(span);
            Debug.Assert((uint)length <= (uint)buffer.Length);
            return new ArraySegment<T>(buffer, 0, length);
        }
    }
}
