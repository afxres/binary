using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Generics.Counters
{
    internal sealed class HashSetCounter<E> : GenericsCounter<HashSet<E>>
    {
        public override int Invoke(HashSet<E> item) => item.Count;
    }
}
