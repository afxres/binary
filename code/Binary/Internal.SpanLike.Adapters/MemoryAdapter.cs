namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using System;

internal sealed class MemoryAdapter<E> : SpanLikeAdapter<Memory<E>, E>
{
    public override ReadOnlySpan<E> Invoke(Memory<E> item)
    {
        return item.Span;
    }
}
