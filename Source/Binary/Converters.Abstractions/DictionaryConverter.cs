using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Converters.Abstractions
{
    internal abstract class DictionaryConverter<TDictionary, TIndex, TValue> : VariableConverter<TDictionary> where TDictionary : IEnumerable<KeyValuePair<TIndex, TValue>>
    {
        private readonly Converter<TIndex> indexConverter;

        private readonly Converter<TValue> valueConverter;

        private readonly int definition;

        protected DictionaryConverter(Converter<TIndex> indexConverter, Converter<TValue> valueConverter)
        {
            this.indexConverter = indexConverter;
            this.valueConverter = valueConverter;
            definition = Define.GetConverterLength(indexConverter, valueConverter);
        }

        protected Dictionary<TIndex, TValue> To(in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return new Dictionary<TIndex, TValue>();
            var itemCount = definition > 0 ? Define.GetItemCount(byteCount, definition) : 0;
            var result = new Dictionary<TIndex, TValue>(itemCount);
            var temp = span;
            while (!temp.IsEmpty)
            {
                var index = indexConverter.ToValueWithMark(ref temp);
                var value = valueConverter.ToValueWithMark(ref temp);
                result.Add(index, value);
            }
            return result;
        }

        public override void ToBytes(ref Allocator allocator, TDictionary item)
        {
            if (item == null)
                return;
            foreach (var i in item)
            {
                indexConverter.ToBytesWithMark(ref allocator, i.Key);
                valueConverter.ToBytesWithMark(ref allocator, i.Value);
            }
        }
    }
}
