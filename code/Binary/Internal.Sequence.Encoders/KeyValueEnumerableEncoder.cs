namespace Mikodev.Binary.Internal.Sequence.Encoders;

using System.Collections.Generic;

internal sealed class KeyValueEnumerableEncoder<T, K, V>(Converter<K> init, Converter<V> tail) where T : IEnumerable<KeyValuePair<K, V>>
{
    private readonly Converter<K> init = init;

    private readonly Converter<V> tail = tail;

    public void Encode(ref Allocator allocator, T? item)
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

    public void EncodeKeyValuePairAuto(ref Allocator allocator, KeyValuePair<K, V> pair)
    {
        var init = this.init;
        var tail = this.tail;
        init.EncodeAuto(ref allocator, pair.Key);
        tail.EncodeAuto(ref allocator, pair.Value);
    }
}
