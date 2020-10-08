using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Sequence
{
    internal static class SequenceKeyValueHelper
    {
        internal static void Encode<K, V>(ref Allocator allocator, Converter<K> init, Converter<V> tail, IEnumerable<KeyValuePair<K, V>> item)
        {
            const int Limits = 8;
            if (item is null)
                return;
            if (item is Dictionary<K, V> { Count: var count } dictionary && count < Limits)
                EncodeDictionary(ref allocator, init, tail, dictionary);
            else if (item is ICollection<KeyValuePair<K, V>> collection)
                EncodeCollection(ref allocator, init, tail, SequenceMethods.GetContents(collection));
            else
                EncodeEnumerable(ref allocator, init, tail, item);
        }

        private static void EncodeDictionary<K, V>(ref Allocator allocator, Converter<K> init, Converter<V> tail, Dictionary<K, V> dictionary)
        {
            foreach (var i in dictionary)
            {
                init.EncodeAuto(ref allocator, i.Key);
                tail.EncodeAuto(ref allocator, i.Value);
            }
        }

        private static void EncodeCollection<K, V>(ref Allocator allocator, Converter<K> init, Converter<V> tail, ReadOnlySpan<KeyValuePair<K, V>> dictionary)
        {
            foreach (var i in dictionary)
            {
                init.EncodeAuto(ref allocator, i.Key);
                tail.EncodeAuto(ref allocator, i.Value);
            }
        }

        private static void EncodeEnumerable<K, V>(ref Allocator allocator, Converter<K> init, Converter<V> tail, IEnumerable<KeyValuePair<K, V>> dictionary)
        {
            foreach (var i in dictionary)
            {
                init.EncodeAuto(ref allocator, i.Key);
                tail.EncodeAuto(ref allocator, i.Value);
            }
        }
    }
}
