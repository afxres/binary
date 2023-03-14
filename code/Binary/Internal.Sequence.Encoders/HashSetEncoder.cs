namespace Mikodev.Binary.Internal.Sequence.Encoders;

using System.Collections.Generic;

internal sealed class HashSetEncoder<E>
{
    private readonly Converter<E> converter;

    public HashSetEncoder(Converter<E> converter) => this.converter = converter;

    public void Encode(ref Allocator allocator, HashSet<E>? item)
    {
        if (item is null)
            return;
        var converter = this.converter;
        foreach (var i in item)
            converter.EncodeAuto(ref allocator, i);
        return;
    }
}
