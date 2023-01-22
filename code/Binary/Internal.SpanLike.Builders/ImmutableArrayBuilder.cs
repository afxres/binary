namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Collections.Immutable;

internal sealed class ImmutableArrayBuilder<E> : SpanLikeBuilder<ImmutableArray<E>, E>
{
    public override ReadOnlySpan<E> Handle(ImmutableArray<E> item)
    {
        return item.AsSpan();
    }

    public override ImmutableArray<E> Invoke(SpanLikeDecoder<E>? decoder, Converter<E> converter, ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return ImmutableArray<E>.Empty;
        return SpanLikeMethods.GetImmutableArray(converter, span);
    }
}
