using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Generics.Counters
{
    internal sealed class CollectionCounter<T, E> : GenericsCounter<T> where T : ICollection<E>
    {
        public override int Invoke(T item) => item.Count;
    }
}
