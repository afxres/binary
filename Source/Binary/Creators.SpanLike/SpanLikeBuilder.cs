using System;

namespace Mikodev.Binary.Creators.SpanLike
{
    internal abstract class SpanLikeBuilder<T, E>
    {
        public abstract ReadOnlySpan<E> Handle(T item);

        public abstract T Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<E> adapter);
    }
}
