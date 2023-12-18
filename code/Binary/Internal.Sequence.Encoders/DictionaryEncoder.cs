namespace Mikodev.Binary.Internal.Sequence.Encoders;

using System.Collections.Generic;

internal sealed class DictionaryEncoder<K, V>(Converter<K> init, Converter<V> tail) where K : notnull
{
    private readonly Converter<K> init = init;

    private readonly Converter<V> tail = tail;

    public void Encode(ref Allocator allocator, Dictionary<K, V>? item)
    {
        if (item is null)
            return;
        var init = this.init;
        var tail = this.tail;
        foreach (var i in item)
        {
            init.EncodeAuto(ref allocator, i.Key);
            tail.EncodeAuto(ref allocator, i.Value);
        }
    }
}
