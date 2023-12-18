namespace Mikodev.Binary.Creators;

using System;
using System.Collections.Generic;

internal sealed class LinkedListConverter<T>(Converter<T> converter) : Converter<LinkedList<T>>
{
    private readonly Converter<T> converter = converter;

    public override void Encode(ref Allocator allocator, LinkedList<T>? item)
    {
        if (item is null)
            return;
        var converter = this.converter;
        for (var i = item.First; i is not null; i = i.Next)
            converter.EncodeAuto(ref allocator, i.Value);
        return;
    }

    public override LinkedList<T> Decode(in ReadOnlySpan<byte> span)
    {
        var converter = this.converter;
        var result = new LinkedList<T>();
        var intent = span;
        while (intent.Length is not 0)
            _ = result.AddLast(converter.DecodeAuto(ref intent));
        return result;
    }
}
