using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Builders
{
    internal sealed class MemoryBuilder<T> : SpanLikeBuilder<Memory<T>, T>
    {
        public override ReadOnlySpan<T> Handle(Memory<T> item) => item.Span;

        public override Memory<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var data = adapter.Decode(span);
            Debug.Assert((uint)data.Length <= (uint)data.Memory.Length);
            return new Memory<T>(data.Memory, 0, data.Length);
        }
    }
}
