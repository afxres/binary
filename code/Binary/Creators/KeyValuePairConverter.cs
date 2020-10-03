using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators
{
    internal sealed class KeyValuePairConverter<K, V> : Converter<KeyValuePair<K, V>>
    {
        private readonly Converter<K> initConverter;

        private readonly Converter<V> tailConverter;

        public KeyValuePairConverter(Converter<K> initConverter, Converter<V> tailConverter, int length) : base(length)
        {
            this.initConverter = initConverter;
            this.tailConverter = tailConverter;
        }

        public override void Encode(ref Allocator allocator, KeyValuePair<K, V> item)
        {
            initConverter.EncodeAuto(ref allocator, item.Key);
            tailConverter.Encode(ref allocator, item.Value);
        }

        public override void EncodeAuto(ref Allocator allocator, KeyValuePair<K, V> item)
        {
            initConverter.EncodeAuto(ref allocator, item.Key);
            tailConverter.EncodeAuto(ref allocator, item.Value);
        }

        public override KeyValuePair<K, V> Decode(in ReadOnlySpan<byte> span)
        {
            var body = span;
            var init = initConverter.DecodeAuto(ref body);
            var tail = tailConverter.Decode(in body);
            return new KeyValuePair<K, V>(init, tail);
        }

        public override KeyValuePair<K, V> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var init = initConverter.DecodeAuto(ref span);
            var tail = tailConverter.DecodeAuto(ref span);
            return new KeyValuePair<K, V>(init, tail);
        }
    }
}
