namespace Mikodev.Binary.Components;

using Mikodev.Binary.Internal.Sequence.Decoders;
using Mikodev.Binary.Internal.SpanLike.Contexts;
using Mikodev.Binary.Internal.SpanLike.Decoders;
using System;
using System.Collections.Generic;

public static class Collection
{
    public static CollectionDecoder<Dictionary<K, V>> GetDictionaryDecoder<K, V>(Converter<K> key, Converter<V> value) where K : notnull
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        var decoder = new DictionaryDecoder<K, V>(key, value);
        return decoder.Invoke;
    }

    public static CollectionDecoder<HashSet<E>> GetHashSetDecoder<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        var decoder = new HashSetDecoder<E>(converter);
        return decoder.Invoke;
    }

    public static CollectionDecoder<List<E>> GetListDecoder<E>(Converter<E> converter)
    {
        ArgumentNullException.ThrowIfNull(converter);
        var decoder = converter is ISpanLikeContextProvider<E> provider ? provider.GetListDecoder() : new ListDecoder<E>(converter);
        return decoder.Invoke;
    }

    public static CollectionDecoder<List<KeyValuePair<K, V>>> GetListDecoder<K, V>(Converter<K> key, Converter<V> value) where K : notnull
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);
        var decoder = new KeyValueEnumerableDecoder<K, V>(key, value);
        return decoder.Invoke;
    }
}
