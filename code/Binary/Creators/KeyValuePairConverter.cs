﻿namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Components;
using System;
using System.Collections.Generic;

internal sealed class KeyValuePairConverter<K, V>(Converter<K> init, Converter<V> tail) : Converter<KeyValuePair<K, V>>(TupleObject.GetConverterLength([init, tail]))
{
    private readonly Converter<K> init = init;

    private readonly Converter<V> tail = tail;

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
