namespace Mikodev.Binary.Internal.SpanLike.Builders;

using System;

internal sealed class ReadOnlyMemoryBuilder<T> : SpanLikeBuilder<ReadOnlyMemory<T>, T>
{
    public override ReadOnlySpan<T> Handle(ReadOnlyMemory<T> item)
    {
        return item.Span;
    }

    public override ReadOnlyMemory<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> invoke)
    {
        return invoke.Decode(span).GetArraySegment();
    }
}
