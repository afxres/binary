using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Components
{
    internal readonly struct DictionaryConverter<T, K, V> where T : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly Converter<KeyValuePair<K, V>> converter;

        public DictionaryConverter(Converter<KeyValuePair<K, V>> converter) => this.converter = converter;

        public void Of(ref Allocator allocator, T item)
        {
            if (item == null)
                return;
            var converter = this.converter;
            foreach (var i in item)
                converter.ToBytesWithMark(ref allocator, i);
        }

        public Dictionary<K, V> To(in ReadOnlySpan<byte> span)
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
    }
}
