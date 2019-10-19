using System.Collections;
using System.Collections.Generic;
using TKey = System.String;
using TValue = Mikodev.Binary.Token;

namespace Mikodev.Binary
{
    public sealed partial class Token : IReadOnlyDictionary<TKey, TValue>
    {
        int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => tokens.Value.Count;

        bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key) => tokens.Value.ContainsKey(key);

        bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => tokens.Value.TryGetValue(key, out value);

        TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => tokens.Value[key];

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => tokens.Value.Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => tokens.Value.Values;

        IEnumerator IEnumerable.GetEnumerator() => tokens.Value.GetEnumerator();

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => tokens.Value.GetEnumerator();
    }
}
