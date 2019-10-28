using System.Collections.Generic;

namespace Mikodev.Binary.CollectionModels.Implementations
{
    internal abstract class DictionaryBuilder<T, K, V> : CollectionBuilder<T, T, Dictionary<K, V>, KeyValuePair<K, V>> { }
}
