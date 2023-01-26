namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using System;

internal sealed class ReadOnlyMemoryAdapter<E> : SpanLikeAdapter<ReadOnlyMemory<E>, E>
{
    public override ReadOnlySpan<E> Invoke(ReadOnlyMemory<E> item)
    {
        return item.Span;
    }
}
