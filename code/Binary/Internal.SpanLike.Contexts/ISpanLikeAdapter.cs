namespace Mikodev.Binary.Internal.SpanLike.Contexts;

using System;

internal interface ISpanLikeAdapter<T, E>
{
    static abstract ReadOnlySpan<E> AsSpan(T? item);

    static abstract T Invoke(E[] values, int length);
}
