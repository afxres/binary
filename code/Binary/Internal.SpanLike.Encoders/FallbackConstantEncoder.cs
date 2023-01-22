namespace Mikodev.Binary.Internal.SpanLike.Encoders;

using System;
using System.Diagnostics;

internal sealed class FallbackConstantEncoder<E> : SpanLikeEncoder<E>
{
    private readonly Converter<E> converter;

    public FallbackConstantEncoder(Converter<E> converter) => this.converter = converter;

    public override void Encode(ref Allocator allocator, ReadOnlySpan<E> item)
    {
        var converter = this.converter;
        Debug.Assert(converter.Length >= 1);
        foreach (var i in item)
            converter.Encode(ref allocator, i);
        return;
    }
}
