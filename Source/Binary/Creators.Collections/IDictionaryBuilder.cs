using Mikodev.Binary.CollectionModels;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IDictionaryBuilder<T, K, V> : CollectionBuilder<T, T, Dictionary<K, V>, KeyValuePair<K, V>> where T : IEnumerable<KeyValuePair<K, V>>
    {
        public override int Length(T item) => item is ICollection<KeyValuePair<K, V>> collection ? collection.Count : NoActualLength;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<Dictionary<K, V>> adapter, in ReadOnlySpan<byte> span) => (T)(object)adapter.To(in span);
    }
}
