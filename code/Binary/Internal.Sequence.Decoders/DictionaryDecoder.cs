namespace Mikodev.Binary.Internal.Sequence.Decoders;

using System;
using System.Collections.Generic;

internal sealed class DictionaryDecoder<K, V> where K : notnull
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
        var capacity = SequenceMethods.GetCapacity<KeyValuePair<K, V>>(limits, this.itemLength, SequenceMethods.FallbackCapacity);
        var init = this.init;
        var tail = this.tail;
        var result = new Dictionary<K, V>(capacity);
        var intent = span;
        while (intent.Length is not 0)
        {
            var head = init.DecodeAuto(ref intent);
            var next = tail.DecodeAuto(ref intent);
            result.Add(head, next);
        }
        return result;
    }
}
