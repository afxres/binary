using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class ArrayLikeAdaptedConverter<T, E> : CollectionAdaptedConverter<T, ReadOnlyMemory<E>, MemoryItem<E>, E>
    {
        public ArrayLikeAdaptedConverter(Converter<E> converter, CollectionBuilder<T, ReadOnlyMemory<E>, MemoryItem<E>, E> builder)
            : base(converter, ArrayLikeAdapterHelper.Create(converter), builder)
        { }
    }
}
