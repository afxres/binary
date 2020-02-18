using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Adapters
{
    internal abstract class DictionaryBuilder<T, K, V> : CollectionBuilder<T, T, Dictionary<K, V>>
    {
        public override int Count(T item) => item is ICollection<KeyValuePair<K, V>> collection ? collection.Count : UnknownCount;

        public override T Of(T item) => item;
    }
}
