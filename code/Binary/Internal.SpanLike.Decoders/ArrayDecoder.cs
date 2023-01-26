namespace Mikodev.Binary.Internal.SpanLike.Decoders;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;

internal sealed class ArrayDecoder<T, E, B> : SpanLikeDecoder<T> where B : struct, ISpanLikeBuilder<T, E>
{
    private readonly Converter<E> converter;

    public ArrayDecoder(Converter<E> converter)
    {
        this.converter = converter;
    }

    public override T Invoke(ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return B.Invoke(Array.Empty<E>(), 0);
        return B.Invoke(SpanLikeMethods.GetArray(this.converter, span, out var actual), actual);
    }
}
