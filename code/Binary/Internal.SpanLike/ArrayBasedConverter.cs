namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;

internal sealed class ArrayBasedConverter<T, E, A>(Converter<E> converter) : Converter<T> where A : ISpanLikeAdapter<T, E>
{
    private readonly Converter<E> converter = converter;

    public override void Encode(ref Allocator allocator, T? item)
    {
        var converter = this.converter;
        var source = A.AsSpan(item);
        foreach (var i in source)
            converter.EncodeAuto(ref allocator, i);
    }

    public override T Decode(in ReadOnlySpan<byte> span)
    {
        var converter = this.converter;
        var values = SpanLikeMethods.GetArray(converter, span, out var actual);
        var result = A.Invoke(values, actual);
        return result;
    }
}
