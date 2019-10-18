using Mikodev.Binary.CollectionAdapters;
using Mikodev.Binary.CollectionAdapters.Collections;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IEnumerableConverter<T, E> : CollectionAdaptedConverter<T, T, ArraySegment<E>, E> where T : IEnumerable<E>
    {
        public IEnumerableConverter(Converter<E> converter)
            : base(converter, new EnumerableAdapter<T, E>(converter), new IEnumerableBuilder<T, E>())
        { }
    }
}
