using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class DictionaryAdapter<T, K, V> : CollectionAdapter<T, Dictionary<K, V>> where T : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly int itemLength;

        private readonly Converter<K> headConverter;

        private readonly Converter<V> dataConverter;

        public DictionaryAdapter(Converter<K> headConverter, Converter<V> dataConverter, int itemLength)
        {
            this.itemLength = itemLength;
            this.headConverter = headConverter;
            this.dataConverter = dataConverter;
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

        public override int Count(T item) => item switch
        {
            null => 0,
            ICollection<KeyValuePair<K, V>> { Count: var alpha } => alpha,
            IReadOnlyCollection<KeyValuePair<K, V>> { Count: var bravo } => bravo,
            _ => -1,
        };

        public override void Of(ref Allocator allocator, T item)
        {
            const int Limits = 8;
            if (item is null)
                return;
            else if (item is Dictionary<K, V> { Count: var count } dictionary && count < Limits)
                foreach (var i in dictionary)
                    AppendAuto(ref allocator, i);
            else
                foreach (var i in item.ToArray())
                    AppendAuto(ref allocator, i);
        }

        public override Dictionary<K, V> To(ReadOnlySpan<byte> span)
        {
            const int Initial = 8;
            var body = span;
            var itemLength = this.itemLength;
            var dictionaryCount = itemLength > 0 ? CollectionAdapterHelper.GetItemCount(body.Length, itemLength, typeof(KeyValuePair<K, V>)) : Initial;
            var dictionary = new Dictionary<K, V>(dictionaryCount);
            while (!body.IsEmpty)
                DetachAuto(ref body, dictionary);
            return dictionary;
        }
    }
}
