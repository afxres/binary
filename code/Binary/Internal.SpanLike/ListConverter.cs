namespace Mikodev.Binary.Internal.SpanLike;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

internal sealed class ListConverter<E>(Converter<E> converter) : Converter<List<E>>
{
    private readonly Converter<E> converter = converter;

    public override void Encode(ref Allocator allocator, List<E>? item)
    {
        var converter = this.converter;
        var source = CollectionsMarshal.AsSpan(item);
        foreach (var i in source)
            converter.EncodeAuto(ref allocator, i);
    }

    public override List<E> Decode(in ReadOnlySpan<byte> span)
    {
        return SpanLikeMethods.GetList(this.converter, span);
    }
}
