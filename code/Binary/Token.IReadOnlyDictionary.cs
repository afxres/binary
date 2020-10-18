using System.Collections;
using System.Collections.Generic;
using Index = System.String;
using Value = Mikodev.Binary.Token;

namespace Mikodev.Binary
{
    public sealed partial class Token : IReadOnlyDictionary<Index, Value>
    {
        int IReadOnlyCollection<KeyValuePair<Index, Value>>.Count => this.lazy.Value.Count;

        bool IReadOnlyDictionary<Index, Value>.ContainsKey(Index key) => this.lazy.Value.ContainsKey(key);

        bool IReadOnlyDictionary<Index, Value>.TryGetValue(Index key, out Value value) => this.lazy.Value.TryGetValue(key, out value);

        IEnumerable<Index> IReadOnlyDictionary<Index, Value>.Keys => this.lazy.Value.Keys;

        IEnumerable<Value> IReadOnlyDictionary<Index, Value>.Values => this.lazy.Value.Values;

        IEnumerator IEnumerable.GetEnumerator() => this.lazy.Value.GetEnumerator();

        IEnumerator<KeyValuePair<Index, Value>> IEnumerable<KeyValuePair<Index, Value>>.GetEnumerator() => this.lazy.Value.GetEnumerator();
    }
}
