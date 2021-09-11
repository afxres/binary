namespace Mikodev.Binary.Internal.SpanLike;

using System;

internal abstract class SpanLikeBuilder<T, E>
{
    public abstract ReadOnlySpan<E> Handle(T? item);

    public abstract T Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<E> invoke);
}
