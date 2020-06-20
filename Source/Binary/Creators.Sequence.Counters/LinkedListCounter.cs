using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Sequence.Counters
{
    internal sealed class LinkedListCounter<E> : SequenceCounter<LinkedList<E>>
    {
        public override int Invoke(LinkedList<E> item) => item.Count;
    }
}
