using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Sequence.Decoders
{
    internal sealed class KeyValueEnumerableDecoder<K, V>
    {
        private readonly int itemLength;

        private readonly Converter<K> init;

        private readonly Converter<V> tail;

        public KeyValueEnumerableDecoder(Converter<K> init, Converter<V> tail, int itemLength)
        {
            this.init = init;
            this.tail = tail;
            this.itemLength = itemLength;
        }

        public IEnumerable<KeyValuePair<K, V>> Decode(ReadOnlySpan<byte> span)
        {
            var limits = span.Length;
            if (limits is 0)
                return Array.Empty<KeyValuePair<K, V>>();
            const int Initial = 8;
            var capacity = SequenceMethods.GetCapacity<KeyValuePair<K, V>>(limits, this.itemLength, Initial);
            var item = new List<KeyValuePair<K, V>>(capacity);
            var body = span;
            var init = this.init;
            var tail = this.tail;
            while (body.Length is not 0)
            {
                var head = init.DecodeAuto(ref body);
                var next = tail.DecodeAuto(ref body);
                item.Add(new KeyValuePair<K, V>(head, next));
            }
            return item;
        }
    }
}
