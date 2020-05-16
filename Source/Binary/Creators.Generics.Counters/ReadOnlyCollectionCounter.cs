using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Generics.Counters
{
    internal sealed class ReadOnlyCollectionCounter<T, E> : GenericsCounter<T> where T : IReadOnlyCollection<E>
    {
        public override int Invoke(T item) => item.Count;
    }
}
