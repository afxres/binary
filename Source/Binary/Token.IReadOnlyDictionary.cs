using System.Collections;
using System.Collections.Generic;
using Index = System.String;
using Value = Mikodev.Binary.Token;

namespace Mikodev.Binary
{
    public sealed partial class Token : IReadOnlyDictionary<Index, Value>
    {
        int IReadOnlyCollection<KeyValuePair<Index, Value>>.Count => tokens.Value.Count;

        bool IReadOnlyDictionary<Index, Value>.ContainsKey(Index key) => tokens.Value.ContainsKey(key);

        bool IReadOnlyDictionary<Index, Value>.TryGetValue(Index key, out Value value) => tokens.Value.TryGetValue(key, out value);

        IEnumerable<Index> IReadOnlyDictionary<Index, Value>.Keys => tokens.Value.Keys;

        IEnumerable<Value> IReadOnlyDictionary<Index, Value>.Values => tokens.Value.Values;

        IEnumerator IEnumerable.GetEnumerator() => tokens.Value.GetEnumerator();

        IEnumerator<KeyValuePair<Index, Value>> IEnumerable<KeyValuePair<Index, Value>>.GetEnumerator() => tokens.Value.GetEnumerator();
    }
}
