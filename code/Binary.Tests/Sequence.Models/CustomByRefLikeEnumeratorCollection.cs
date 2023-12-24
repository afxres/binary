namespace Mikodev.Binary.Tests.Sequence.Models;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public sealed class CustomByRefLikeEnumeratorCollection<T>(IEnumerable<T> items) : IEnumerable<T>
{
    public List<T> Items { get; } = items.ToList();

    public int CurrentCallCount { get; private set; } = 0;

    public int MoveNextCallCount { get; private set; } = 0;

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public ref struct Enumerator(CustomByRefLikeEnumeratorCollection<T> source)
    {
        private readonly CustomByRefLikeEnumeratorCollection<T> source = source;

        private int index = -1;

        public readonly T Current
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
