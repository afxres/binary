using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.CollectionModels.Implementations
{
    internal sealed class DictionaryAdapter<T, K, V> : CollectionAdapter<T, Dictionary<K, V>, KeyValuePair<K, V>> where T : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly Converter<KeyValuePair<K, V>> converter;

        public DictionaryAdapter(Converter<KeyValuePair<K, V>> converter) => this.converter = converter;

        public override void Of(ref Allocator allocator, T item)
        {
            if (item == null)
                return;
            var converter = this.converter;
            foreach (var i in item)
                converter.EncodeAuto(ref allocator, i);
        }

        public override Dictionary<K, V> To(in ReadOnlySpan<byte> span)
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
                Add(data, converter.DecodeAuto(ref temp));
            return data;
        }
    }
}
