﻿namespace Mikodev.Binary.Internal.Sequence.Decoders;

using Mikodev.Binary.Components;
using System;
using System.Collections.Generic;

internal sealed class KeyValueEnumerableDecoder<K, V>
{
    private readonly int itemLength;

    private readonly Converter<K> init;

    private readonly Converter<V> tail;

    public KeyValueEnumerableDecoder(Converter<K> init, Converter<V> tail)
    {
        this.init = init;
        this.tail = tail;
        this.itemLength = TupleObject.GetTupleObjectLength(new IConverter[] { init, tail });
    }

    public List<KeyValuePair<K, V>> Invoke(ReadOnlySpan<byte> span)
    {
        var limits = span.Length;
        if (limits is 0)
            return new List<KeyValuePair<K, V>>();
        var capacity = SequenceContext.GetCapacityOrDefault<KeyValuePair<K, V>>(limits, this.itemLength);
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
