using Mikodev.Binary.Internal;

namespace Mikodev.Binary.CollectionModels
{
    internal abstract class EnumerableBuilder<T, E> : CollectionBuilder<T, T, MemoryItem<E>, E> { }
}
