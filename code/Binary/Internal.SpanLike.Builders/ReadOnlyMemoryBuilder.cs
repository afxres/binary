namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike;
using System;

internal sealed class ReadOnlyMemoryBuilder<E> : SpanLikeBuilder<ReadOnlyMemory<E>, E>
{
    public override ReadOnlySpan<E> Handle(ReadOnlyMemory<E> item)
    {
        return item.Span;
    }

    public override ReadOnlyMemory<E> Invoke(SpanLikeDecoder<E>? decoder, Converter<E> converter, ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return new ReadOnlyMemory<E>(Array.Empty<E>());
        if (decoder is null)
            return new ReadOnlyMemory<E>(SpanLikeMethods.GetArray(converter, span, out var actual), 0, actual);
        var result = default(E[]);
        decoder.Decode(ArrayDecoderContext<E>.Instance, ref result, span);
        return new ReadOnlyMemory<E>(result);
    }
}
