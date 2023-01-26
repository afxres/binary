namespace Mikodev.Binary.Internal.Sequence.Decoders;

using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Collections.Generic;
using System.Diagnostics;

internal sealed class EnumerableDecoder<T, E> where T : IEnumerable<E>
{
    private readonly Converter<E> converter;

    private readonly SpanLikeDecoder<E[]>? decoder;

    public EnumerableDecoder(Converter<E> converter)
    {
        Debug.Assert(converter is not null);
        this.decoder = SpanLikeContext.GetDecoderOrDefault<E[], E>(converter);
        this.converter = converter;
    }

    public T Decode(ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return (T)(object)Array.Empty<E>();
        var decoder = this.decoder;
        if (decoder is not null)
            return (T)(object)decoder.Invoke(span);
        return (T)(object)SpanLikeMethods.GetList(this.converter, span);
    }
}
