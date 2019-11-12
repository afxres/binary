using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Adapters
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

        public override Dictionary<K, V> To(ReadOnlySpan<byte> span)
        {
            static void Add(Dictionary<K, V> items, KeyValuePair<K, V> item) => items.Add(item.Key, item.Value);

            var byteLength = span.Length;
            if (byteLength == 0)
                return new Dictionary<K, V>();
            const int Initial = 8;
            var converter = this.converter;
            var converterLength = converter.Length;
            var itemCount = converterLength > 0 ? CollectionAdapterHelper.GetItemCount(byteLength, converterLength, typeof(KeyValuePair<K, V>)) : Initial;
            var items = new Dictionary<K, V>(itemCount);
            var body = span;
            while (!body.IsEmpty)
                Add(items, converter.DecodeAuto(ref body));
            return items;
        }
    }
}
