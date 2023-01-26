namespace Mikodev.Binary.Internal.SpanLike;

using System;

internal abstract class SpanLikeAdapter<T, E>
{
    public abstract ReadOnlySpan<E> Invoke(T? item);
}
