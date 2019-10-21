using Mikodev.Binary.CollectionModels;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal abstract class ArrayLikeBuilder<T, E> : CollectionBuilder<T, ReadOnlyMemory<E>, ArraySegment<E>, E> { }
}
