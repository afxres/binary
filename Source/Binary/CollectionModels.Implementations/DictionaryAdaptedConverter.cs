using System.Collections.Generic;

namespace Mikodev.Binary.CollectionModels.Implementations
{
    internal sealed class DictionaryAdaptedConverter<T, K, V> : CollectionAdaptedConverter<T, T, Dictionary<K, V>, KeyValuePair<K, V>> where T : IEnumerable<KeyValuePair<K, V>>
    {
        public DictionaryAdaptedConverter(Converter<KeyValuePair<K, V>> converter, CollectionBuilder<T, T, Dictionary<K, V>, KeyValuePair<K, V>> builder)
            : base(converter, new DictionaryAdapter<T, K, V>(converter), builder)
        { }
    }
}
