using System;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.SpanLike.Builders
{
    internal sealed class ReadOnlyMemoryBuilder<T> : SpanLikeBuilder<ReadOnlyMemory<T>, T>
    {
        public override ReadOnlySpan<T> Handle(ReadOnlyMemory<T> item) => item.Span;

        public override ReadOnlyMemory<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var data = adapter.Decode(span);
            Debug.Assert((uint)data.Length <= (uint)data.Memory.Length);
            return new ReadOnlyMemory<T>(data.Memory, 0, data.Length);
        }
    }
}
