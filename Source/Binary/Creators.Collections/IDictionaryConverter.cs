using Mikodev.Binary.CollectionAdapters;
using Mikodev.Binary.CollectionAdapters.Collections;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IDictionaryConverter<T, K, V> : CollectionAdaptedConverter<T, T, Dictionary<K, V>, KeyValuePair<K, V>> where T : IEnumerable<KeyValuePair<K, V>>
    {
        public IDictionaryConverter(Converter<KeyValuePair<K, V>> converter)
            : base(converter, new DictionaryAdapter<T, K, V>(converter), new IDictionaryBuilder<T, K, V>())
        { }
    }
}
