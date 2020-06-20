using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Sequence.Counters
{
    internal sealed class ReadOnlyCollectionCounter<T, E> : SequenceCounter<T> where T : IReadOnlyCollection<E>
    {
        public override int Invoke(T item) => item.Count;
    }
}
