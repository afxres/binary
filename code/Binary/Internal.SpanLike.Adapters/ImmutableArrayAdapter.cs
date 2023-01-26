namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using System;
using System.Collections.Immutable;

internal sealed class ImmutableArrayAdapter<E> : SpanLikeAdapter<ImmutableArray<E>, E>
{
    public override ReadOnlySpan<E> Invoke(ImmutableArray<E> item)
    {
        return item.AsSpan();
    }
}
