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

        private readonly Converter<K> initConverter;

        private readonly Converter<V> tailConverter;

        public DictionaryAdapter(Converter<K> initConverter, Converter<V> tailConverter, int itemLength)
        {
            this.itemLength = itemLength;
            this.initConverter = initConverter;
            this.tailConverter = tailConverter;
            Debug.Assert(itemLength > 0 || (itemLength == 0 && (initConverter.Length == 0 || tailConverter.Length == 0)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EncodeAutoInternal(ref Allocator allocator, KeyValuePair<K, V> pair)
        {
            initConverter.EncodeAuto(ref allocator, pair.Key);
            tailConverter.EncodeAuto(ref allocator, pair.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DecodeAutoInternal(ref ReadOnlySpan<byte> span, Dictionary<K, V> dictionary)
        {
            var init = initConverter.DecodeAuto(ref span);
            var tail = tailConverter.DecodeAuto(ref span);
            dictionary.Add(init, tail);
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
            if (item is Dictionary<K, V> { Count: var count } dictionary && count < Limits)
                foreach (var i in dictionary)
                    EncodeAutoInternal(ref allocator, i);
            else
                foreach (var i in item.ToArray())
                    EncodeAutoInternal(ref allocator, i);
        }

        public override Dictionary<K, V> To(ReadOnlySpan<byte> span)
        {
            const int Initial = 8;
            var body = span;
            var itemLength = this.itemLength;
            var dictionaryCount = itemLength > 0 ? CollectionAdapterHelper.GetItemCount(body.Length, itemLength, typeof(KeyValuePair<K, V>)) : Initial;
            var dictionary = new Dictionary<K, V>(dictionaryCount);
            while (!body.IsEmpty)
                DecodeAutoInternal(ref body, dictionary);
            return dictionary;
        }
    }
}
