namespace Mikodev.Binary.Internal.Sequence.Decoders;

using System;
using System.Collections.Generic;

internal sealed class DictionaryDecoder<K, V>
{
    private readonly int itemLength;

    private readonly Converter<K> init;

    private readonly Converter<V> tail;

    public DictionaryDecoder(Converter<K> init, Converter<V> tail, int itemLength)
    {
        this.init = init;
        this.tail = tail;
        this.itemLength = itemLength;
    }

    public Dictionary<K, V> Decode(ReadOnlySpan<byte> span)
    {
        var limits = span.Length;
        if (limits is 0)
            return new Dictionary<K, V>();
        const int Capacity = 8;
        var capacity = SequenceMethods.GetCapacity<KeyValuePair<K, V>>(limits, this.itemLength, Capacity);
        var item = new Dictionary<K, V>(capacity);
        var body = span;
        var init = this.init;
        var tail = this.tail;
        while (body.Length is not 0)
        {
            var head = init.DecodeAuto(ref body);
            var next = tail.DecodeAuto(ref body);
            item.Add(head, next);
        }
        return item;
    }
}
