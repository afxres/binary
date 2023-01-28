namespace Mikodev.Binary.Internal.SpanLike.Contexts;

using System;

internal interface ISpanLikeAdapter<T, E>
{
    static abstract ReadOnlySpan<E> AsSpan(T? item);

    static abstract int Length(T? item);

    static abstract void Encode(ref Allocator allocator, T? item, Converter<E> converter);
}
