using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators
{
    internal sealed class KeyValuePairConverter<K, V> : Converter<KeyValuePair<K, V>>
    {
        private readonly Converter<K> headConverter;

        private readonly Converter<V> dataConverter;

        public KeyValuePairConverter(Converter<K> headConverter, Converter<V> dataConverter, int length) : base(length)
        {
            this.headConverter = headConverter;
            this.dataConverter = dataConverter;
        }

        public override void Encode(ref Allocator allocator, KeyValuePair<K, V> item)
        {
            headConverter.EncodeAuto(ref allocator, item.Key);
            dataConverter.Encode(ref allocator, item.Value);
        }

        public override KeyValuePair<K, V> Decode(in ReadOnlySpan<byte> span)
        {
            var body = span;
            var head = headConverter.DecodeAuto(ref body);
            var data = dataConverter.Decode(in body);
            return new KeyValuePair<K, V>(head, data);
        }

        public override void EncodeAuto(ref Allocator allocator, KeyValuePair<K, V> item)
        {
            headConverter.EncodeAuto(ref allocator, item.Key);
            dataConverter.EncodeAuto(ref allocator, item.Value);
        }

        public override KeyValuePair<K, V> DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var head = headConverter.DecodeAuto(ref span);
            var data = dataConverter.DecodeAuto(ref span);
            return new KeyValuePair<K, V>(head, data);
        }
    }
}
