namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;

internal readonly struct MemoryAdapter<E> : ISpanLikeAdapter<Memory<E>, E>
{
    public static ReadOnlySpan<E> AsSpan(Memory<E> item)
    {
        return item.Span;
    }

    public static Memory<E> Invoke(E[] values, int length)
    {
        return new Memory<E>(values, 0, length);
    }
}
