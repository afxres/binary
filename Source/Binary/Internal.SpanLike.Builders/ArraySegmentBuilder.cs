using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Builders
{
    internal sealed class ArraySegmentBuilder<T> : SpanLikeBuilder<ArraySegment<T>, T>
    {
        public override ReadOnlySpan<T> Handle(ArraySegment<T> item) => item;

        public override ArraySegment<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var data = adapter.Decode(span);
            Debug.Assert((uint)data.Length <= (uint)data.Memory.Length);
            return new ArraySegment<T>(data.Memory, 0, data.Length);
        }
    }
}
