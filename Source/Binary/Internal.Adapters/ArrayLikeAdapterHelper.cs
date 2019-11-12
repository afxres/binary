using Mikodev.Binary.Creators;
using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal static class ArrayLikeAdapterHelper
    {
        internal static ArrayLikeAdapter<T> Create<T>(Converter<T> converter)
        {
            static object Invoke(Converter<T> converter)
            {
                if (converter.GetType().IsImplementationOf(typeof(OriginalEndiannessConverter<>)))
                    return Activator.CreateInstance(typeof(ArrayLikeOriginalEndiannessAdapter<>).MakeGenericType(typeof(T)));
                if (converter.Length > 0)
                    return new ArrayLikeConstantAdapter<T>(converter);
                else
                    return new ArrayLikeVariableAdapter<T>(converter);
            }
            return (ArrayLikeAdapter<T>)Invoke(converter);
        }
    }
}
