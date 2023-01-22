namespace Mikodev.Binary.Internal.Sequence.Decoders;

using Mikodev.Binary.Internal.SpanLike;
using Mikodev.Binary.Internal.SpanLike.Builders;
using System;
using System.Collections.Generic;

internal sealed class EnumerableDecoder<T, E> where T : IEnumerable<E>
{
    private readonly Converter<E> converter;

    private readonly SpanLikeDecoder<E>? decoder;

    public EnumerableDecoder(Converter<E> converter)
    {
        var decoder = SpanLikeContext.GetDecoderOrDefault(converter);
        this.decoder = decoder;
        this.converter = converter;
    }

    public T Decode(ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return (T)(object)Array.Empty<E>();
        var decoder = this.decoder;
        if (decoder is null)
            return (T)(object)SpanLikeMethods.GetList(this.converter, span);
        var result = default(E[]);
        decoder.Decode(ArrayDecoderContext<E>.Instance, ref result, span);
        return (T)(object)result;
    }
}
