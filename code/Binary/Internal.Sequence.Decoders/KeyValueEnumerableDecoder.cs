namespace Mikodev.Binary.Internal.Sequence.Decoders;

using System;
using System.Collections.Generic;

internal sealed class KeyValueEnumerableDecoder<K, V>
{
    private readonly int itemLength;

    private readonly Converter<K> init;

    private readonly Converter<V> tail;

    public KeyValueEnumerableDecoder(Converter<K> init, Converter<V> tail, int itemLength)
    {
        this.init = init;
        this.tail = tail;
        this.itemLength = itemLength;
    }

    public IEnumerable<KeyValuePair<K, V>> Decode(ReadOnlySpan<byte> span)
    {
        var limits = span.Length;
        if (limits is 0)
            return Array.Empty<KeyValuePair<K, V>>();
        var capacity = SequenceMethods.GetCapacity<KeyValuePair<K, V>>(limits, this.itemLength, SequenceMethods.FallbackCapacity);
        var init = this.init;
        var tail = this.tail;
        var result = new List<KeyValuePair<K, V>>(capacity);
        var intent = span;
        while (intent.Length is not 0)
        {
            var head = init.DecodeAuto(ref intent);
            var next = tail.DecodeAuto(ref intent);
            result.Add(new KeyValuePair<K, V>(head, next));
        }
        return result;
    }
}
