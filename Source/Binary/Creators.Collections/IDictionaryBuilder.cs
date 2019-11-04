using Mikodev.Binary.Internal.Adapters;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IDictionaryBuilder<T, K, V> : DictionaryBuilder<T, K, V> where T : IEnumerable<KeyValuePair<K, V>>
    {
        public override int Count(T item) => item is ICollection<KeyValuePair<K, V>> collection ? collection.Count : NoActualLength;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<Dictionary<K, V>> adapter, ReadOnlySpan<byte> span) => (T)(object)adapter.To(span);
    }
}
