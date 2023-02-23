namespace Mikodev.Binary.Tests.Sequence.Models;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public sealed class CustomValueEnumeratorDictionary<K, V> : IReadOnlyDictionary<K, V>
{
    public List<KeyValuePair<K, V>> Items { get; }

    public int CurrentCallCount { get; private set; } = 0;

    public int MoveNextCallCount { get; private set; } = 0;

    public CustomValueEnumeratorDictionary(IEnumerable<KeyValuePair<K, V>> list)
    {
        Items = list.ToList();
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public struct Enumerator
    {
        private readonly CustomValueEnumeratorDictionary<K, V> source;

        private int index;

        public Enumerator(CustomValueEnumeratorDictionary<K, V> source) : this()
        {
            this.source = source;
            this.index = -1;
        }

        public KeyValuePair<K, V> Current
        {
            get
            {
                this.source.CurrentCallCount += 1;
                return this.source.Items[this.index];
            }
        }

        public bool MoveNext()
        {
            this.source.MoveNextCallCount += 1;
            var index = this.index + 1;
            if ((uint)index >= (uint)this.source.Items.Count)
                return false;
            this.index = index;
            return true;
        }
    }

    bool IReadOnlyDictionary<K, V>.ContainsKey(K key) => throw new NotSupportedException();

    bool IReadOnlyDictionary<K, V>.TryGetValue(K key, out V value) => throw new NotSupportedException();

    V IReadOnlyDictionary<K, V>.this[K key] => throw new NotSupportedException();

    IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => throw new NotSupportedException();

    IEnumerable<V> IReadOnlyDictionary<K, V>.Values => throw new NotSupportedException();

    int IReadOnlyCollection<KeyValuePair<K, V>>.Count => throw new NotSupportedException();

    IEnumerator<KeyValuePair<K, V>> IEnumerable<KeyValuePair<K, V>>.GetEnumerator() => throw new NotSupportedException();

    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
}
