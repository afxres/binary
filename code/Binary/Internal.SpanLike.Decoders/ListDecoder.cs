namespace Mikodev.Binary.Internal.SpanLike.Decoders;

using System;
using System.Collections.Generic;

internal sealed class ListDecoder<E>(Converter<E> converter) : SpanLikeDecoder<List<E>>
{
    private readonly Converter<E> converter = converter;

    public override List<E> Invoke(ReadOnlySpan<byte> span)
    {
        return SpanLikeMethods.GetList(this.converter, span);
    }
}
