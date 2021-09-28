namespace Mikodev.Binary.Internal.SpanLike.Builders;

using System;

internal sealed class ArraySegmentBuilder<T> : SpanLikeBuilder<ArraySegment<T>, T>
{
    public override ReadOnlySpan<T> Handle(ArraySegment<T> item)
    {
        return item;
    }

    public override ArraySegment<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> invoke)
    {
        return invoke.Decode(span).GetArraySegment();
    }
}
