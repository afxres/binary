namespace Mikodev.Binary.Creators;

using System;
using System.Collections.Generic;

internal sealed class LinkedListConverter<T> : Converter<LinkedList<T>>
{
    private readonly Converter<T> converter;

    public LinkedListConverter(Converter<T> converter) => this.converter = converter;

    public override void Encode(ref Allocator allocator, LinkedList<T>? item)
    {
        if (item is null)
            return;
        var converter = this.converter;
        for (var i = item.First; i is not null; i = i.Next)
            converter.EncodeAuto(ref allocator, i.Value);
    }

    public override LinkedList<T> Decode(in ReadOnlySpan<byte> span)
    {
        var body = span;
        var list = new LinkedList<T>();
        var converter = this.converter;
        while (body.Length is not 0)
            _ = list.AddLast(converter.DecodeAuto(ref body));
        return list;
    }
}
