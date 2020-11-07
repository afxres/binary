using Mikodev.Binary.Creators;
using Mikodev.Binary.Internal.SpanLike.Adapters;
using System;

namespace Mikodev.Binary.Internal.SpanLike
{
    internal static class SpanLikeAdapterHelper
    {
        internal static SpanLikeAdapter<T> Create<T>(Converter<T> converter)
        {
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(converter.GetType(), x => x == typeof(NativeEndianConverter<>)))
                return (SpanLikeAdapter<T>)Activator.CreateInstance(typeof(NativeEndianAdapter<>).MakeGenericType(typeof(T)));
            if (converter.Length > 0)
                return new ConstantAdapter<T>(converter);
            else
                return new VariableAdapter<T>(converter);
        }
    }
}
