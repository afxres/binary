using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class DictionaryAdaptedConverter<T, K, V> : CollectionAdaptedConverter<T, T, Dictionary<K, V>, KeyValuePair<K, V>> where T : IEnumerable<KeyValuePair<K, V>>
    {
        public DictionaryAdaptedConverter(CollectionBuilder<T, T, Dictionary<K, V>> builder, Converter<K> headConverter, Converter<V> dataConverter, int itemLength)
            : base(new DictionaryAdapter<T, K, V>(headConverter, dataConverter, itemLength), builder, itemLength)
        { }
    }
}
