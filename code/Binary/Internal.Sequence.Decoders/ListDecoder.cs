namespace Mikodev.Binary.Internal.Sequence.Decoders;

using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Collections.Generic;

internal sealed class ListDecoder<E>(Converter<E> converter)
{
    private readonly Converter<E> converter = converter;

    public List<E> Invoke(ReadOnlySpan<byte> span)
    {
        return SpanLikeMethods.GetList(this.converter, span);
    }
}
