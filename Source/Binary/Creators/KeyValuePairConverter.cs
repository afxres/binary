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

        public override void ToBytes(ref Allocator allocator, KeyValuePair<K, V> item)
        {
            converterK.ToBytesWithMark(ref allocator, item.Key);
            converterV.ToBytes(ref allocator, item.Value);
        }

        public override KeyValuePair<K, V> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var head = converterK.ToValueWithMark(ref temp);
            var last = converterV.ToValue(in temp);
            return new KeyValuePair<K, V>(head, last);
        }

        public override void ToBytesWithMark(ref Allocator allocator, KeyValuePair<K, V> item)
        {
            converterK.ToBytesWithMark(ref allocator, item.Key);
            converterV.ToBytesWithMark(ref allocator, item.Value);
        }

        public override KeyValuePair<K, V> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var head = converterK.ToValueWithMark(ref span);
            var last = converterV.ToValueWithMark(ref span);
            return new KeyValuePair<K, V>(head, last);
        }
    }
}
