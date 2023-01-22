namespace Mikodev.Binary.Internal.SpanLike.Encoders;

using System;
using System.Diagnostics;

internal sealed class FallbackVariableEncoder<E> : SpanLikeEncoder<E>
{
    private readonly Converter<E> converter;

    public FallbackVariableEncoder(Converter<E> converter) => this.converter = converter;

    public override void Encode(ref Allocator allocator, ReadOnlySpan<E> item)
    {
        var converter = this.converter;
        Debug.Assert(converter.Length is 0);
        foreach (var i in item)
            converter.EncodeAuto(ref allocator, i);
        return;
    }
}
