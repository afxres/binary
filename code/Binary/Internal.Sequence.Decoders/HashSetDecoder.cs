namespace Mikodev.Binary.Internal.Sequence.Decoders;

using System;
using System.Collections.Generic;

internal sealed class HashSetDecoder<E>(Converter<E> converter)
{
    private readonly Converter<E> converter = converter;

    public HashSet<E> Invoke(ReadOnlySpan<byte> span)
    {
        var converter = this.converter;
        var result = new HashSet<E>();
        var intent = span;
        while (intent.Length is not 0)
            _ = result.Add(converter.DecodeAuto(ref intent));
        return result;
    }
}
