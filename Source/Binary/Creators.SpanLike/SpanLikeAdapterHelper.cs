using Mikodev.Binary.Creators.SpanLike.Adapters;
using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.SpanLike
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
