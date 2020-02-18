using Mikodev.Binary.Creators;
using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal static class ArrayLikeAdapterHelper
    {
        internal static ArrayLikeAdapter<T> Create<T>(Converter<T> converter)
        {
            if (converter.GetType().IsImplementationOf(typeof(OriginalEndiannessConverter<>)))
                return (ArrayLikeAdapter<T>)Activator.CreateInstance(typeof(ArrayLikeOriginalEndiannessAdapter<>).MakeGenericType(typeof(T)));
            if (converter.Length > 0)
                return new ArrayLikeConstantAdapter<T>(converter);
            else
                return new ArrayLikeVariableAdapter<T>(converter);
        }
    }
}
