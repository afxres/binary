using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Sequence.Adapters
{
    internal sealed class KeyValueEnumerableAdapter<T, K, V> : SequenceAdapter<T, IEnumerable<KeyValuePair<K, V>>> where T : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly int itemLength;

        private readonly Converter<K> initConverter;

        private readonly Converter<V> tailConverter;

        public KeyValueEnumerableAdapter(Converter<K> initConverter, Converter<V> tailConverter, int itemLength)
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

        public override IEnumerable<KeyValuePair<K, V>> Decode(ReadOnlySpan<byte> span)
        {
            var byteLength = span.Length;
            if (byteLength is 0)
                return Array.Empty<KeyValuePair<K, V>>();
            const int Initial = 8;
            var capacity = SequenceMethods.GetCapacity<KeyValuePair<K, V>>(byteLength, itemLength, Initial);
            var item = new List<KeyValuePair<K, V>>(capacity);
            var body = span;
            var init = initConverter;
            var tail = tailConverter;
            while (body.IsEmpty is false)
            {
                var head = init.DecodeAuto(ref body);
                var next = tail.DecodeAuto(ref body);
                item.Add(new KeyValuePair<K, V>(head, next));
            }
            return item;
        }
    }
}
