namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;

internal readonly struct ReadOnlyMemoryAdapter<E> : ISpanLikeAdapter<ReadOnlyMemory<E>, E>
{
    public static ReadOnlySpan<E> AsSpan(ReadOnlyMemory<E> item)
    {
        return item.Span;
    }

    public static ReadOnlyMemory<E> Invoke(E[] values, int length)
    {
        return new ReadOnlyMemory<E>(values, 0, length);
    }
}
