﻿using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class EnumerableAdaptedConverter<T, E> : CollectionAdaptedConverter<T, T, MemoryItem<E>, E> where T : IEnumerable<E>
    {
        public EnumerableAdaptedConverter(Converter<E> converter, CollectionBuilder<T, T, MemoryItem<E>> builder)
            : base(converter, new EnumerableAdapter<T, E>(converter), builder)
        { }
    }
}
