using Mikodev.Binary.CollectionAdapters.ArrayLike;
using Mikodev.Binary.Internal.Extensions;
using System;

namespace Mikodev.Binary.CollectionAdapters
{
    internal static class CollectionAdapterHelper
    {
        internal static CollectionAdapter<ReadOnlyMemory<T>, T> Create<T>(Converter<T> converter)
        {
            var adapter = converter.IsOriginalEndiannessConverter()
                ? Activator.CreateInstance(typeof(OriginalEndiannessCollectionAdapter<>).MakeGenericType(converter.ItemType))
                : Activator.CreateInstance((converter.Length > 0 ? typeof(ConstantCollectionAdapter<>) : typeof(VariableCollectionAdapter<>)).MakeGenericType(converter.ItemType), converter);
            return (CollectionAdapter<ReadOnlyMemory<T>, T>)adapter;
        }
    }
}
