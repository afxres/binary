namespace Mikodev.Binary.Benchmarks.EnumerationTests.Models;

using System.Collections.Generic;

public static class EnumerationEncoder
{
    public static void EncodeHashSetForEach<T>(ref Allocator allocator, Converter<T> converter, HashSet<T>? collection)
    {
        if (collection is null)
            return;
        foreach (var i in collection)
            converter.Encode(ref allocator, i);
    }

    public static void EncodeEnumerableForEach<T>(ref Allocator allocator, Converter<T> converter, IEnumerable<T>? collection)
    {
        if (collection is null)
            return;
        foreach (var i in collection)
            converter.Encode(ref allocator, i);
    }

    public static void EncodeCollectionToArrayThenForEach<T>(ref Allocator allocator, Converter<T> converter, ICollection<T>? collection)
    {
        if (collection is null)
            return;
        var array = new T[collection.Count];
        collection.CopyTo(array, 0);
        foreach (var i in array)
            converter.Encode(ref allocator, i);
    }

    public static void EncodeDictionaryForEach<K, V>(ref Allocator allocator, Converter<K> init, Converter<V> tail, Dictionary<K, V>? collection) where K : notnull
    {
        if (collection is null)
            return;
        foreach (var i in collection)
        {
            init.Encode(ref allocator, i.Key);
            tail.Encode(ref allocator, i.Value);
        }
    }

    public static void EncodeKeyValueEnumerableForEach<K, V>(ref Allocator allocator, Converter<K> init, Converter<V> tail, IEnumerable<KeyValuePair<K, V>>? collection)
    {
        if (collection is null)
            return;
        foreach (var i in collection)
        {
            init.Encode(ref allocator, i.Key);
            tail.Encode(ref allocator, i.Value);
        }
    }

    public static void EncodeKeyValueCollectionForEach<K, V>(ref Allocator allocator, Converter<K> init, Converter<V> tail, ICollection<KeyValuePair<K, V>>? collection)
    {
        if (collection is null)
            return;
        var array = new KeyValuePair<K, V>[collection.Count];
        collection.CopyTo(array, 0);
        foreach (var i in array)
        {
            init.Encode(ref allocator, i.Key);
            tail.Encode(ref allocator, i.Value);
        }
    }
}
