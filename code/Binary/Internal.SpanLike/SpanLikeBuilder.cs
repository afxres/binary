using System;

namespace Mikodev.Binary.Internal.SpanLike
{
    internal abstract class SpanLikeBuilder<T, E>
    {
        public abstract ReadOnlySpan<E> Handle(T item);

        public abstract T Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<E> invoke);
    }
}
