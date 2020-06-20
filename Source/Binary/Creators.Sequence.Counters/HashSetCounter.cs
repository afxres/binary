using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Sequence.Counters
{
    internal sealed class HashSetCounter<E> : SequenceCounter<HashSet<E>>
    {
        public override int Invoke(HashSet<E> item) => item.Count;
    }
}
