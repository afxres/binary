namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Converters.Endianness;
using Mikodev.Binary.Internal.SpanLike.Adapters;

internal static class SpanLikeAdapter
{
    internal static SpanLikeAdapter<T> Create<T>(Converter<T> converter)
    {
        if (CommonModule.SelectGenericTypeDefinitionOrDefault(converter.GetType(), x => x == typeof(NativeEndianConverter<>)))
            return (SpanLikeAdapter<T>)CommonModule.CreateInstance(typeof(NativeEndianAdapter<>).MakeGenericType(typeof(T)), null);
        if (converter.Length > 0)
            return new ConstantAdapter<T>(converter);
        else
            return new VariableAdapter<T>(converter);
    }
}
