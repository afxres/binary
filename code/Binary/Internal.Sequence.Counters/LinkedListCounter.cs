using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Sequence.Counters
{
    internal sealed class LinkedListCounter<E> : SequenceCounter<LinkedList<E>>
    {
        public override int Invoke(LinkedList<E> item) => item.Count;
    }
}
