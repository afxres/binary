using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Sequence.Adapters
{
    internal sealed class KeyValueEnumerableAdapter<T, K, V> : SequenceAdapter<T, IEnumerable<KeyValuePair<K, V>>> where T : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly int itemLength;

        private readonly Converter<K> init;

        private readonly Converter<V> tail;

        public KeyValueEnumerableAdapter(Converter<K> init, Converter<V> tail, int itemLength)
        {
            this.init = init;
            this.tail = tail;
            this.itemLength = itemLength;
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            SequenceKeyValueHelper.Encode(ref allocator, this.init, this.tail, item);
        }

        public override IEnumerable<KeyValuePair<K, V>> Decode(ReadOnlySpan<byte> span)
        {
            var byteLength = span.Length;
            if (byteLength is 0)
                return Array.Empty<KeyValuePair<K, V>>();
            const int Initial = 8;
            var capacity = SequenceMethods.GetCapacity<KeyValuePair<K, V>>(byteLength, this.itemLength, Initial);
            var item = new List<KeyValuePair<K, V>>(capacity);
            var body = span;
            var init = this.init;
            var tail = this.tail;
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
