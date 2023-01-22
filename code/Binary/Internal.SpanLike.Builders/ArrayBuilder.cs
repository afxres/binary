namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike;
using System;

internal sealed class ArrayBuilder<E> : SpanLikeBuilder<E[], E>
{
    public override ReadOnlySpan<E> Handle(E[]? item)
    {
        return item;
    }

    public override E[] Invoke(SpanLikeDecoder<E>? decoder, Converter<E> converter, ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return Array.Empty<E>();
        if (decoder is null)
            return SpanLikeMethods.GetArray(converter, span);
        var result = default(E[]);
        decoder.Decode(ArrayDecoderContext<E>.Instance, ref result, span);
        return result;
    }
}
