namespace Mikodev.Binary.Tests.Sequence.Models;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public sealed class CustomByRefLikeEnumeratorCollection<T> : IEnumerable<T>
{
    public List<T> Items { get; }

    public int CurrentCallCount { get; private set; } = 0;

    public int MoveNextCallCount { get; private set; } = 0;

    public CustomByRefLikeEnumeratorCollection(IEnumerable<T> items)
    {
        Items = items.ToList();
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public ref struct Enumerator
    {
        private readonly CustomByRefLikeEnumeratorCollection<T> source;

        private int index;

        public Enumerator(CustomByRefLikeEnumeratorCollection<T> source)
        {
            this.index = -1;
            this.source = source;
        }

        public T Current
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

    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotSupportedException();
}
