namespace Mikodev.Binary.Internal.Sequence.Encoders;

using System.Collections.Generic;

internal sealed class EnumerableEncoder<T, E>(Converter<E> converter) where T : IEnumerable<E>
{
    private readonly Converter<E> converter = converter;

    public void Encode(ref Allocator allocator, T? item)
    {
        if (item is null)
            return;
        var converter = this.converter;
        foreach (var i in item)
            converter.EncodeAuto(ref allocator, i);
    }
}
