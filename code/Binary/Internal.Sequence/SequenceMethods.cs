namespace Mikodev.Binary.Internal.Sequence;

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;

internal static class SequenceMethods
{
    internal static FrozenDictionary<K, V> GetFrozenDictionary<K, V>(IEnumerable<KeyValuePair<K, V>> items) where K : notnull
    {
        Debug.Assert(items is not null);
        Debug.Assert(items is List<KeyValuePair<K, V>>);
        return FrozenDictionary.ToFrozenDictionary(items, true);
    }

    internal static FrozenSet<T> GetFrozenSet<T>(IEnumerable<T> items)
    {
        Debug.Assert(items is not null);
        Debug.Assert(items is T[] or List<T>);
        return FrozenSet.ToFrozenSet(items, true);
    }
}
