using Mikodev.Binary.CollectionModels;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArrayLikeConverter<T, E> : CollectionAdaptedConverter<T, ReadOnlyMemory<E>, ArraySegment<E>, E>
    {
        public ArrayLikeConverter(Converter<E> converter, CollectionBuilder<T, ReadOnlyMemory<E>, ArraySegment<E>, E> builder)
            : base(converter, CollectionAdapterHelper.Create(converter), builder)
        { }
    }
}
