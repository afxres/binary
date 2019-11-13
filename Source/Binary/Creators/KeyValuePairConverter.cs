using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators
{
    internal sealed class KeyValuePairConverter<K, V> : Converter<KeyValuePair<K, V>>
    {
        private readonly Converter<K> converterK;

        private readonly Converter<V> converterV;

        public KeyValuePairConverter(Converter<K> converterK, Converter<V> converterV, int length) : base(length)
        {
            this.converterK = converterK;
            this.converterV = converterV;
        }

        public override void Encode(ref Allocator allocator, KeyValuePair<K, V> item)
        {
            converterK.EncodeAuto(ref allocator, item.Key);
            converterV.Encode(ref allocator, item.Value);
        }

        public override KeyValuePair<K, V> Decode(in ReadOnlySpan<byte> span)
        {
            var body = span;
            var head = converterK.DecodeAuto(ref body);
            var tail = converterV.Decode(in body);
            return new KeyValuePair<K, V>(head, tail);
        }

        public override void EncodeAuto(ref Allocator allocator, KeyValuePair<K, V> item)
        {
            converterK.EncodeAuto(ref allocator, item.Key);
            converterV.EncodeAuto(ref allocator, item.Value);
        }

        public override KeyValuePair<K, V> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var head = converterK.DecodeAuto(ref span);
            var tail = converterV.DecodeAuto(ref span);
            return new KeyValuePair<K, V>(head, tail);
        }
    }
}
