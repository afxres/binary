﻿using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Sequence.Encoders
{
    internal sealed class KeyValueEnumerableEncoder<T, K, V> where T : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly Converter<K> init;

        private readonly Converter<V> tail;

        public KeyValueEnumerableEncoder(Converter<K> init, Converter<V> tail)
        {
            this.init = init;
            this.tail = tail;
        }

        public void Encode(ref Allocator allocator, T item)
        {
            if (item is null)
                return;
            var init = this.init;
            var tail = this.tail;
            foreach (var i in item)
            {
                init.EncodeAuto(ref allocator, i.Key);
                tail.EncodeAuto(ref allocator, i.Value);
            }
        }
    }
}
