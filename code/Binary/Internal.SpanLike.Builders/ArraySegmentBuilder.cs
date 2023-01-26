namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;

internal readonly struct ArraySegmentBuilder<E> : ISpanLikeBuilder<ArraySegment<E>, E>
{
    public static ArraySegment<E> Invoke(E[] array, int count)
    {
        return new ArraySegment<E>(array, 0, count);
    }
}
