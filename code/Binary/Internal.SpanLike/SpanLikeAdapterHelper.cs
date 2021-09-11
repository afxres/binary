namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Converters.Endianness;
using Mikodev.Binary.Internal.SpanLike.Adapters;

internal static class SpanLikeAdapterHelper
{
    internal static SpanLikeAdapter<T> Create<T>(Converter<T> converter)
    {
        if (CommonHelper.SelectGenericTypeDefinitionOrDefault(converter.GetType(), x => x == typeof(NativeEndianConverter<>)))
            return (SpanLikeAdapter<T>)CommonHelper.CreateInstance(typeof(NativeEndianAdapter<>).MakeGenericType(typeof(T)), null);
        if (converter.Length > 0)
            return new ConstantAdapter<T>(converter);
        else
            return new VariableAdapter<T>(converter);
    }
}
