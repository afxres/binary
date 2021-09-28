namespace Mikodev.Binary.Internal.SpanLike.Builders;

using System;

internal sealed class MemoryBuilder<T> : SpanLikeBuilder<Memory<T>, T>
{
    public override ReadOnlySpan<T> Handle(Memory<T> item)
    {
        return item.Span;
    }

    public override Memory<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> invoke)
    {
        return invoke.Decode(span).GetArraySegment();
    }
}
