using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Sequence.Counters
{
    internal sealed class CollectionCounter<T, E> : SequenceCounter<T> where T : ICollection<E>
    {
        public override int Invoke(T item) => item.Count;
    }
}
