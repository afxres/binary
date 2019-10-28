using Mikodev.Binary.Internal.Extensions;
using System;

namespace Mikodev.Binary.CollectionModels.ArrayLike
{
    internal static class ArrayLikeAdapterHelper
    {
        internal static ArrayLikeAdapter<T> Create<T>(Converter<T> converter)
        {
            var flag = converter.IsOriginalEndiannessConverter();
            var adapterDefinition = flag
                ? typeof(OriginalEndiannessCollectionAdapter<>)
                : converter.Length > 0 ? typeof(ConstantCollectionAdapter<>) : typeof(VariableCollectionAdapter<>);
            var adapterType = adapterDefinition.MakeGenericType(converter.ItemType);
            var adapterArguments = flag ? Array.Empty<object>() : new object[] { converter };
            var adapter = Activator.CreateInstance(adapterType, adapterArguments);
            return (ArrayLikeAdapter<T>)adapter;
        }
    }
}
