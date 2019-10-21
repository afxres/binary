using Mikodev.Binary.CollectionModels.ArrayLikeAdapters;
using Mikodev.Binary.Internal.Extensions;
using System;

namespace Mikodev.Binary.CollectionModels
{
    internal static class CollectionAdapterHelper
    {
        internal static CollectionAdapter<ReadOnlyMemory<T>, ArraySegment<T>, T> Create<T>(Converter<T> converter)
        {
            var adapter = converter.IsOriginalEndiannessConverter()
                ? Activator.CreateInstance(typeof(OriginalEndiannessCollectionAdapter<>).MakeGenericType(converter.ItemType))
                : Activator.CreateInstance((converter.Length > 0 ? typeof(ConstantCollectionAdapter<>) : typeof(VariableCollectionAdapter<>)).MakeGenericType(converter.ItemType), converter);
            return (CollectionAdapter<ReadOnlyMemory<T>, ArraySegment<T>, T>)adapter;
        }
    }
}
