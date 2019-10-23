using Mikodev.Binary.CollectionModels;
using Mikodev.Binary.CollectionModels.ArrayLikeAdapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArrayLikeConverter<T, E> : CollectionAdaptedConverter<T, ReadOnlyMemory<E>, ArraySegment<E>, E>
    {
        public ArrayLikeConverter(Converter<E> converter, CollectionBuilder<T, ReadOnlyMemory<E>, ArraySegment<E>, E> builder)
            : base(converter, ArrayLikeAdapterHelper.Create(converter), builder)
        { }
    }
}
