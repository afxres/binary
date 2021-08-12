namespace Mikodev.Binary.Creators;

using System;
using System.Collections.Generic;

internal sealed class KeyValuePairConverter<K, V> : Converter<KeyValuePair<K, V>>
{
    private readonly Converter<K> init;

    private readonly Converter<V> tail;

    public KeyValuePairConverter(Converter<K> init, Converter<V> tail, int itemLength) : base(itemLength)
    {
        this.init = init;
        this.tail = tail;
    }

    public override void Encode(ref Allocator allocator, KeyValuePair<K, V> item)
    {
        this.init.EncodeAuto(ref allocator, item.Key);
        this.tail.Encode(ref allocator, item.Value);
    }

    public override void EncodeAuto(ref Allocator allocator, KeyValuePair<K, V> item)
    {
        this.init.EncodeAuto(ref allocator, item.Key);
        this.tail.EncodeAuto(ref allocator, item.Value);
    }

    public override KeyValuePair<K, V> Decode(in ReadOnlySpan<byte> span)
    {
        var body = span;
        var head = this.init.DecodeAuto(ref body);
        var next = this.tail.Decode(in body);
        return new KeyValuePair<K, V>(head, next);
    }

    public override KeyValuePair<K, V> DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        var head = this.init.DecodeAuto(ref span);
        var next = this.tail.DecodeAuto(ref span);
        return new KeyValuePair<K, V>(head, next);
    }
}
