namespace Mikodev.Binary.SourceGeneration.Internal;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

public static class CollectionExtensions
{
    public static bool TryAdd<K, V>(this IDictionary<K, V> dictionary, K key, V value)
    {
        if (dictionary.ContainsKey(key))
            return false;
        dictionary.Add(key, value);
        return true;
    }

    public static void ForEach<T>(this ImmutableArray<T> values, Action<T> action)
    {
        foreach (var i in values)
            action.Invoke(i);
        return;
    }

    public static void ForEach<T>(this ImmutableArray<T> values, Action<int, T> action)
    {
        for (var i = 0; i < values.Length; i++)
            action.Invoke(i, values[i]);
        return;
    }
}
