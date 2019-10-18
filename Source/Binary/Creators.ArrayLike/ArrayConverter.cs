using Mikodev.Binary.CollectionAdapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArrayConverter<T> : CollectionAdaptedConverter<T[], ReadOnlyMemory<T>, T>
    {
        public ArrayConverter(Converter<T> converter)
            : base(converter, CollectionAdapterHelper.Create(converter), new ArrayBuilder<T>())
        { }
    }
}
