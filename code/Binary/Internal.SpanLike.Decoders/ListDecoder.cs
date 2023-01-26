namespace Mikodev.Binary.Internal.SpanLike.Decoders;

using System;
using System.Collections.Generic;

internal sealed class ListDecoder<E> : SpanLikeDecoder<List<E>>
{
    private readonly Converter<E> converter;

    public ListDecoder(Converter<E> converter)
    {
        this.converter = converter;
    }

    public override List<E> Invoke(ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return new List<E>();
        return SpanLikeMethods.GetList(this.converter, span);
    }
}
