using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators
{
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
            init.EncodeAuto(ref allocator, item.Key);
            tail.Encode(ref allocator, item.Value);
        }

        public override void EncodeAuto(ref Allocator allocator, KeyValuePair<K, V> item)
        {
            init.EncodeAuto(ref allocator, item.Key);
            tail.EncodeAuto(ref allocator, item.Value);
        }

        public override KeyValuePair<K, V> Decode(in ReadOnlySpan<byte> span)
        {
            var body = span;
            var head = init.DecodeAuto(ref body);
            var next = tail.Decode(in body);
            return new KeyValuePair<K, V>(head, next);
        }

        public override KeyValuePair<K, V> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var head = init.DecodeAuto(ref span);
            var next = tail.DecodeAuto(ref span);
            return new KeyValuePair<K, V>(head, next);
        }
    }
}
