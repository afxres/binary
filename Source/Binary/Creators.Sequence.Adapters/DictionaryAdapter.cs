using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Creators.Sequence.Adapters
{
    internal sealed class DictionaryAdapter<T, K, V> : SequenceAdapter<T, Dictionary<K, V>> where T : IEnumerable<KeyValuePair<K, V>>
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

        public override void Encode(ref Allocator allocator, T item)
        {
            const int Limits = 8;
            if (item is null)
                return;
            if (item is Dictionary<K, V> { Count: var count } dictionary && count < Limits)
                foreach (var i in dictionary)
                    EncodeAutoInternal(ref allocator, i);
            else if (item is ICollection<KeyValuePair<K, V>> collection)
                foreach (var i in SequenceMethods.GetContents(collection))
                    EncodeAutoInternal(ref allocator, i);
            else
                foreach (var i in item)
                    EncodeAutoInternal(ref allocator, i);
        }

        public override Dictionary<K, V> Decode(ReadOnlySpan<byte> span)
        {
            var byteLength = span.Length;
            if (byteLength == 0)
                return new Dictionary<K, V>();
            const int Initial = 8;
            var itemLength = this.itemLength;
            var capacity = itemLength > 0 ? SequenceMethods.GetCapacity<KeyValuePair<K, V>>(byteLength, itemLength) : Initial;
            var collection = new Dictionary<K, V>(capacity);
            var body = span;
            while (!body.IsEmpty)
                DecodeAutoInternal(ref body, collection);
            return collection;
        }
    }
}
