namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;

internal readonly struct ArraySegmentAdapter<E> : ISpanLikeAdapter<ArraySegment<E>, E>
{
    public static ReadOnlySpan<E> AsSpan(ArraySegment<E> item)
    {
        return item;
    }

    public static ArraySegment<E> Invoke(E[] values, int length)
    {
        return new ArraySegment<E>(values, 0, length);
    }
}
