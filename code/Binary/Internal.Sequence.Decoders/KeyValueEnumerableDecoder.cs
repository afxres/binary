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
        const int Capacity = 8;
        var capacity = SequenceMethods.GetCapacity<KeyValuePair<K, V>>(limits, this.itemLength, Capacity);
        var memory = new MemoryBuffer<KeyValuePair<K, V>>(capacity);
        var body = span;
        var init = this.init;
        var tail = this.tail;
        while (body.Length is not 0)
        {
            var head = init.DecodeAuto(ref body);
            var next = tail.DecodeAuto(ref body);
            memory.Add(new KeyValuePair<K, V>(head, next));
        }
        return memory.GetEnumerable();
    }
}
