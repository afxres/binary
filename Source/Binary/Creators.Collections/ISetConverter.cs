using Mikodev.Binary.CollectionAdapters;
using Mikodev.Binary.CollectionAdapters.Collections;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ISetConverter<T, E> : CollectionAdaptedConverter<T, T, ArraySegment<E>, E> where T : ISet<E>
    {
        public ISetConverter(Converter<E> converter)
            : base(converter, new EnumerableAdapter<T, E>(converter), new ISetBuilder<T, E>())
        { }
    }
}
