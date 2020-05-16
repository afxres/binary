using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Generics.Counters
{
    internal sealed class LinkedListCounter<E> : GenericsCounter<LinkedList<E>>
    {
        public override int Invoke(LinkedList<E> item) => item.Count;
    }
}
