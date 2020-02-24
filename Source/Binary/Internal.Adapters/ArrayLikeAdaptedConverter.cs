using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class ArrayLikeAdaptedConverter<T, E> : CollectionAdaptedConverter<T, ReadOnlyMemory<E>, ArraySegment<E>, E>
    {
        public ArrayLikeAdaptedConverter(CollectionBuilder<T, ReadOnlyMemory<E>, ArraySegment<E>> builder, Converter<E> converter)
            : base(ArrayLikeAdapterHelper.Create(converter), builder, converter.Length)
        { }
    }
}
