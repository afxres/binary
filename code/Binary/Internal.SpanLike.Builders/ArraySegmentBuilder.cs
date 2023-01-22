namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike;
using System;

internal sealed class ArraySegmentBuilder<E> : SpanLikeBuilder<ArraySegment<E>, E>
{
    public override ReadOnlySpan<E> Handle(ArraySegment<E> item)
    {
        return item;
    }

    public override ArraySegment<E> Invoke(SpanLikeDecoder<E>? decoder, Converter<E> converter, ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return new ArraySegment<E>(Array.Empty<E>());
        if (decoder is null)
            return new ArraySegment<E>(SpanLikeMethods.GetArray(converter, span, out var actual), 0, actual);
        var result = default(E[]);
        decoder.Decode(ArrayDecoderContext<E>.Instance, ref result, span);
        return new ArraySegment<E>(result);
    }
}
