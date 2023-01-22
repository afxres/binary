namespace Mikodev.Binary.Internal.SpanLike;

using System;

internal abstract class SpanLikeEncoder<E>
{
    public abstract void Encode(ref Allocator allocator, ReadOnlySpan<E> item);
}
