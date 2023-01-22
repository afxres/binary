namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike;
using System;

internal sealed class MemoryBuilder<E> : SpanLikeBuilder<Memory<E>, E>
{
    public override ReadOnlySpan<E> Handle(Memory<E> item)
    {
        return item.Span;
    }

    public override Memory<E> Invoke(SpanLikeDecoder<E>? decoder, Converter<E> converter, ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return new Memory<E>(Array.Empty<E>());
        if (decoder is null)
            return new Memory<E>(SpanLikeMethods.GetArray(converter, span, out var actual), 0, actual);
        var result = default(E[]);
        decoder.Decode(ArrayDecoderContext<E>.Instance, ref result, span);
        return new Memory<E>(result);
    }
}
