namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

internal sealed class ListBuilder<E> : SpanLikeBuilder<List<E>, E>
{
    public override ReadOnlySpan<E> Handle(List<E>? item)
    {
        return CollectionsMarshal.AsSpan(item);
    }

    public override List<E> Invoke(SpanLikeDecoder<E>? decoder, Converter<E> converter, ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return new List<E>();
        return SpanLikeMethods.GetList(converter, span);
    }
}
