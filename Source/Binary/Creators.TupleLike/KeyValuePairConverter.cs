using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.TupleLike
{
    internal sealed class KeyValuePairConverter<TIndex, TValue> : Converter<KeyValuePair<TIndex, TValue>>
    {
        private readonly Converter<TIndex> indexConverter;

        private readonly Converter<TValue> valueConverter;

        public KeyValuePairConverter(
            Converter<TIndex> indexConverter,
            Converter<TValue> valueConverter,
            int length) : base(length)
        {
            this.indexConverter = indexConverter;
            this.valueConverter = valueConverter;
        }

        public sealed override void ToBytes(ref Allocator allocator, KeyValuePair<TIndex, TValue> item)
        {
            indexConverter.ToBytesWithMark(ref allocator, item.Key);
            valueConverter.ToBytes(ref allocator, item.Value);
        }

        public sealed override KeyValuePair<TIndex, TValue> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var index = indexConverter.ToValueWithMark(ref temp);
            var value = valueConverter.ToValue(in temp);
            return new KeyValuePair<TIndex, TValue>(index, value);
        }

        public sealed override void ToBytesWithMark(ref Allocator allocator, KeyValuePair<TIndex, TValue> item)
        {
            indexConverter.ToBytesWithMark(ref allocator, item.Key);
            valueConverter.ToBytesWithMark(ref allocator, item.Value);
        }

        public sealed override KeyValuePair<TIndex, TValue> ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var index = indexConverter.ToValueWithMark(ref span);
            var value = valueConverter.ToValueWithMark(ref span);
            return new KeyValuePair<TIndex, TValue>(index, value);
        }
    }
}
