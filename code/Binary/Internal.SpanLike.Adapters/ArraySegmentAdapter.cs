namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using System;

internal sealed class ArraySegmentAdapter<E> : SpanLikeAdapter<ArraySegment<E>, E>
{
    public override ReadOnlySpan<E> Invoke(ArraySegment<E> item)
    {
        return item;
    }
}
