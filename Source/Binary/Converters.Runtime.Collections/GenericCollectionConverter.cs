using Mikodev.Binary.CollectionAdapters;
using Mikodev.Binary.CollectionAdapters.Collections;
using Mikodev.Binary.Internal.Delegates;
using System.Collections.Generic;

namespace Mikodev.Binary.Converters.Runtime.Collections
{
    internal sealed class GenericCollectionConverter<T, E> : CollectionAdaptedConverter<T, T, E> where T : IEnumerable<E>
    {
        public GenericCollectionConverter(ToCollection<T, E> constructor, Converter<E> converter, bool reverse)
            : base(converter, new EnumerableAdapter<T, E>(converter), new GenericCollectionBuilder<T, E>(constructor, reverse))
        { }
    }
}
