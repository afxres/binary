using Mikodev.Binary.Internal.Adapters;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IDictionaryBuilder<T, K, V> : DictionaryBuilder<T, K, V> where T : IEnumerable<KeyValuePair<K, V>>
    {
        public override T To(CollectionAdapter<Dictionary<K, V>> adapter, ReadOnlySpan<byte> span) => (T)(object)adapter.To(span);
    }
}
