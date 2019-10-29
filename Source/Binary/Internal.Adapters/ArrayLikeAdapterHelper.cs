using Mikodev.Binary.Internal.Extensions;
using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal static class ArrayLikeAdapterHelper
    {
        internal static ArrayLikeAdapter<T> Create<T>(Converter<T> converter)
        {
            var flag = converter.IsOriginalEndiannessConverter();
            var adapterDefinition = flag
                ? typeof(ArrayLikeOriginalEndiannessAdapter<>)
                : converter.Length > 0 ? typeof(ArrayLikeConstantAdapter<>) : typeof(ArrayLikeVariableAdapter<>);
            var adapterType = adapterDefinition.MakeGenericType(converter.ItemType);
            var adapterArguments = flag ? Array.Empty<object>() : new object[] { converter };
            var adapter = Activator.CreateInstance(adapterType, adapterArguments);
            return (ArrayLikeAdapter<T>)adapter;
        }
    }
}
