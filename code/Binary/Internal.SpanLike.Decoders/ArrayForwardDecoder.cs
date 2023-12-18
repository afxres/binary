namespace Mikodev.Binary.Internal.SpanLike.Decoders;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Diagnostics;

internal sealed class ArrayForwardDecoder<T, E, B>(SpanLikeDecoder<E[]> decoder) : SpanLikeDecoder<T> where B : struct, ISpanLikeBuilder<T, E>
{
    private readonly SpanLikeDecoder<E[]> decoder = decoder;

    public override T Invoke(ReadOnlySpan<byte> span)
    {
        var result = this.decoder.Invoke(span);
        Debug.Assert(result.Length is not 0 || ReferenceEquals(result, Array.Empty<E>()));
        return B.Invoke(result, result.Length);
    }
}
