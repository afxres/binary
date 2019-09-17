using System.Collections;
using System.Collections.Generic;
using TKey = System.String;
using TValue = Mikodev.Binary.Token;

namespace Mikodev.Binary
{
    public sealed partial class Token : IReadOnlyDictionary<TKey, TValue>
    {
        int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => GetTokens().Count;

        bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key) => GetTokens().ContainsKey(key);

        bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => GetTokens().TryGetValue(key, out value);

        TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => GetTokens()[key];

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => GetTokens().Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => GetTokens().Values;

        IEnumerator IEnumerable.GetEnumerator() => GetTokens().GetEnumerator();

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => GetTokens().GetEnumerator();
    }
}
