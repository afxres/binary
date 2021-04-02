using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Builders
{
    internal sealed class ArraySegmentBuilder<T> : SpanLikeBuilder<ArraySegment<T>, T>
    {
        public override ReadOnlySpan<T> Handle(ArraySegment<T> item) => item;

        public override ArraySegment<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var result = adapter.Decode(span);
            Debug.Assert((uint)result.Length <= (uint)result.Memory.Length);
            return new ArraySegment<T>(result.Memory, 0, result.Length);
        }
    }
}
