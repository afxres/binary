namespace Mikodev.Binary.Internal.Sequence.Decoders;

using System;
using System.Collections.Generic;

internal sealed class HashSetDecoder<E>
{
    private readonly Converter<E> converter;

    public HashSetDecoder(Converter<E> converter) => this.converter = converter;

    public HashSet<E> Decode(ReadOnlySpan<byte> span)
    {
        var body = span;
        var item = new HashSet<E>();
        var converter = this.converter;
        while (body.Length is not 0)
            _ = item.Add(converter.DecodeAuto(ref body));
        return item;
    }
}
