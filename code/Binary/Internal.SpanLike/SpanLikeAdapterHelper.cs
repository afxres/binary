using Mikodev.Binary.Creators;
using Mikodev.Binary.Internal.SpanLike.Adapters;
using System;

namespace Mikodev.Binary.Internal.SpanLike
{
    internal static class SpanLikeAdapterHelper
    {
        internal static SpanLikeAdapter<T> Create<T>(Converter<T> converter)
        {
            if (CommonHelper.IsImplementationOf(converter.GetType(), typeof(NativeEndianConverter<>)))
                return (SpanLikeAdapter<T>)Activator.CreateInstance(typeof(SpanLikeNativeEndianAdapter<>).MakeGenericType(typeof(T)));
            if (converter.Length > 0)
                return new SpanLikeConstantAdapter<T>(converter);
            else
                return new SpanLikeVariableAdapter<T>(converter);
        }
    }
}
