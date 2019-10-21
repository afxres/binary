using System;
using System.Collections.Generic;

namespace Mikodev.Binary.CollectionModels.Implementations
{
    internal sealed class EnumerableAdaptedConverter<T, E> : CollectionAdaptedConverter<T, T, ArraySegment<E>, E> where T : IEnumerable<E>
    {
        public EnumerableAdaptedConverter(Converter<E> converter, CollectionBuilder<T, T, ArraySegment<E>, E> builder)
            : base(converter, new EnumerableAdapter<T, E>(converter), builder)
        { }
    }
}
