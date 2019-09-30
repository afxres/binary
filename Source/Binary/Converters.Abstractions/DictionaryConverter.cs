using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Converters.Abstractions
{
    internal abstract class DictionaryConverter<T, K, V> : VariableConverter<T> where T : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly Converter<KeyValuePair<K, V>> converter;

        protected DictionaryConverter(Converter<KeyValuePair<K, V>> converter) => this.converter = converter;

        protected Dictionary<K, V> To(in ReadOnlySpan<byte> span)
        {
            static void Add(Dictionary<K, V> data, KeyValuePair<K, V> item) => data.Add(item.Key, item.Value);

            var byteCount = span.Length;
            if (byteCount == 0)
                return new Dictionary<K, V>();
            const int InitialCapacity = 8;
            var converter = this.converter;
            var converterLength = converter.Length;
            var itemCount = converterLength > 0 ? CollectionHelper.GetItemCount(byteCount, converterLength) : InitialCapacity;
            var data = new Dictionary<K, V>(itemCount);
            var temp = span;
            while (!temp.IsEmpty)
                Add(data, converter.ToValueWithMark(ref temp));
            return data;
        }

        public override void ToBytes(ref Allocator allocator, T item)
        {
            if (item == null)
                return;
            var converter = this.converter;
            foreach (var i in item)
                converter.ToBytesWithMark(ref allocator, i);
        }
    }
}
