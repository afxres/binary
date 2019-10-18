using Mikodev.Binary.CollectionAdapters;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ListConverter<T> : CollectionAdaptedConverter<List<T>, ReadOnlyMemory<T>, T>
    {
        public ListConverter(Converter<T> converter, CollectionBuilder<List<T>, ReadOnlyMemory<T>, T> builder)
            : base(converter, CollectionAdapterHelper.Create(converter), builder)
        { }
    }
}
