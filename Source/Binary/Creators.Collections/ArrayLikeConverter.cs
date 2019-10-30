using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using System;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ArrayLikeConverter<T, E> : CollectionAdaptedConverter<T, ReadOnlyMemory<E>, MemoryItem<E>, E>
    {
        public ArrayLikeConverter(Converter<E> converter, CollectionBuilder<T, ReadOnlyMemory<E>, MemoryItem<E>, E> builder)
            : base(converter, ArrayLikeAdapterHelper.Create(converter), builder)
        { }
    }
}
