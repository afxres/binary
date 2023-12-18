namespace Mikodev.Binary.Internal.SpanLike.Decoders;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Diagnostics;

internal sealed class ArrayDecoder<T, E, B>(Converter<E> converter) : SpanLikeDecoder<T> where B : struct, ISpanLikeBuilder<T, E>
{
    private readonly Converter<E> converter = converter;

    public override T Invoke(ReadOnlySpan<byte> span)
    {
        var result = SpanLikeMethods.GetArray(this.converter, span, out var actual);
        Debug.Assert(result.Length is not 0 || ReferenceEquals(result, Array.Empty<E>()));
        return B.Invoke(result, actual);
    }
}
