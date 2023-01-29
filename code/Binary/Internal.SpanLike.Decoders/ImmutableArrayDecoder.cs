namespace Mikodev.Binary.Internal.SpanLike.Decoders;

using System;
using System.Collections.Immutable;

internal sealed class ImmutableArrayDecoder<E> : SpanLikeDecoder<ImmutableArray<E>>
{
    private readonly Converter<E> converter;

    public ImmutableArrayDecoder(Converter<E> converter)
    {
        this.converter = converter;
    }

    public override ImmutableArray<E> Invoke(ReadOnlySpan<byte> span)
    {
        return SpanLikeMethods.GetImmutableArray(this.converter, span);
    }
}
