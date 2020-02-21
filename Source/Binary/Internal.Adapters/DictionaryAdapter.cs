using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class DictionaryAdapter<T, K, V> : CollectionAdapter<T, Dictionary<K, V>> where T : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly Converter<K> headConverter;

        private readonly Converter<V> dataConverter;

        private readonly int itemLength;

        public DictionaryAdapter(Converter<K> headConverter, Converter<V> dataConverter, int itemLength)
        {
            this.headConverter = headConverter;
            this.dataConverter = dataConverter;
            this.itemLength = itemLength;
            Debug.Assert(itemLength > 0 || (itemLength == 0 && (headConverter.Length == 0 || dataConverter.Length == 0)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AppendAuto(ref Allocator allocator, KeyValuePair<K, V> pair)
        {
            headConverter.EncodeAuto(ref allocator, pair.Key);
            dataConverter.EncodeAuto(ref allocator, pair.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DetachAuto(ref ReadOnlySpan<byte> span, Dictionary<K, V> dictionary)
        {
            var head = headConverter.DecodeAuto(ref span);
            var data = dataConverter.DecodeAuto(ref span);
            dictionary.Add(head, data);
        }

        public override void Of(ref Allocator allocator, T item)
        {
            if (item is null)
                return;
            else if (item is Dictionary<K, V> dictionary)
                foreach (var i in dictionary)
                    AppendAuto(ref allocator, i);
            else
                foreach (var i in item)
                    AppendAuto(ref allocator, i);
        }

        public override Dictionary<K, V> To(ReadOnlySpan<byte> span)
        {
            var byteLength = span.Length;
            if (byteLength == 0)
                return new Dictionary<K, V>();
            const int Initial = 8;
            var itemLength = this.itemLength;
            var dictionaryCount = itemLength > 0 ? CollectionAdapterHelper.GetItemCount(byteLength, itemLength, typeof(KeyValuePair<K, V>)) : Initial;
            var dictionary = new Dictionary<K, V>(dictionaryCount);
            var body = span;
            while (!body.IsEmpty)
                DetachAuto(ref body, dictionary);
            return dictionary;
        }
    }
}
