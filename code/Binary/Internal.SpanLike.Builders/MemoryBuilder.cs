using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Builders
{
    internal sealed class MemoryBuilder<T> : SpanLikeBuilder<Memory<T>, T>
    {
        public override ReadOnlySpan<T> Handle(Memory<T> item) => item.Span;

        public override Memory<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> invoke)
        {
            var (buffer, length) = invoke.Decode(span);
            Debug.Assert((uint)length <= (uint)buffer.Length);
            return new Memory<T>(buffer, 0, length);
        }
    }
}
