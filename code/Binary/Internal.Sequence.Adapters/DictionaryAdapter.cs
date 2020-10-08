﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Sequence.Adapters
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
            Debug.Assert(itemLength > 0 || initConverter.Length is 0 || tailConverter.Length is 0);
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            SequenceKeyValueHelper.Encode(ref allocator, initConverter, tailConverter, item);
        }

        public override Dictionary<K, V> Decode(ReadOnlySpan<byte> span)
        {
            var byteLength = span.Length;
            if (byteLength is 0)
                return new Dictionary<K, V>();
            const int Initial = 8;
            var capacity = SequenceMethods.GetCapacity<KeyValuePair<K, V>>(byteLength, itemLength, Initial);
            var item = new Dictionary<K, V>(capacity);
            var body = span;
            var init = initConverter;
            var tail = tailConverter;
            while (body.IsEmpty is false)
            {
                var head = init.DecodeAuto(ref body);
                var next = tail.DecodeAuto(ref body);
                item.Add(head, next);
            }
            return item;
        }
    }
}
