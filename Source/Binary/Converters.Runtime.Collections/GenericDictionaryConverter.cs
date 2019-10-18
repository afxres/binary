using Mikodev.Binary.CollectionAdapters;
using Mikodev.Binary.CollectionAdapters.Collections;
using Mikodev.Binary.Internal.Delegates;
using System.Collections.Generic;

namespace Mikodev.Binary.Converters.Runtime.Collections
{
    internal sealed class GenericDictionaryConverter<T, K, V> : CollectionAdaptedConverter<T, T, Dictionary<K, V>, KeyValuePair<K, V>> where T : IEnumerable<KeyValuePair<K, V>>
    {
        public GenericDictionaryConverter(ToDictionary<T, K, V> constructor, Converter<KeyValuePair<K, V>> converter)
            : base(converter, new DictionaryAdapter<T, K, V>(converter), new GenericDictionaryBuilder<T, K, V>(constructor))
        { }
    }
}
