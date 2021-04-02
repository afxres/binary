using System;
using System.Collections.Generic;
using System.Diagnostics;

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
            var memory = new MemoryBuffer<KeyValuePair<K, V>>(capacity);
            var body = span;
            var init = this.init;
            var tail = this.tail;
            while (body.Length is not 0)
            {
                var head = init.DecodeAuto(ref body);
                var next = tail.DecodeAuto(ref body);
                memory.Append(new KeyValuePair<K, V>(head, next));
            }
            var result = memory.Result();
            Debug.Assert((uint)result.Length <= (uint)result.Memory.Length);
            var buffer = result.Memory;
            var length = result.Length;
            if (buffer.Length == length)
                return buffer;
            return new ArraySegment<KeyValuePair<K, V>>(result.Memory, 0, result.Length);
        }
    }
}
