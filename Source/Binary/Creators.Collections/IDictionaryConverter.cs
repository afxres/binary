using Mikodev.Binary.CollectionAdapters;
using Mikodev.Binary.CollectionAdapters.Collections;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IDictionaryBuilder<T, K, V> : CollectionBuilder<T, T, Dictionary<K, V>, KeyValuePair<K, V>> where T : IEnumerable<KeyValuePair<K, V>>
    {
        public override int Length(T item) => item is ICollection<KeyValuePair<K, V>> collection ? collection.Count : NoActualLength;

        public override T Of(T item) => item;

        public override T To(CollectionAdapter<Dictionary<K, V>, KeyValuePair<K, V>> adapter, in ReadOnlySpan<byte> span) => (T)(object)adapter.To(in span);
    }

    internal sealed class IDictionaryConverter<T, K, V> : CollectionAdaptedConverter<T, T, Dictionary<K, V>, KeyValuePair<K, V>> where T : IEnumerable<KeyValuePair<K, V>>
    {
        public IDictionaryConverter(Converter<KeyValuePair<K, V>> converter)
            : base(converter, new DictionaryAdapter<T, K, V>(converter), new IDictionaryBuilder<T, K, V>())
        { }
    }
}
