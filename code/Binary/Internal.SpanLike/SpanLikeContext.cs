namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using Mikodev.Binary.Internal.SpanLike.Encoders;

internal static class SpanLikeContext
{
    internal static SpanLikeEncoder<E> GetEncoder<E>(Converter<E> converter)
    {
        if (converter is ISpanLikeEncoderProvider<E> provider)
            return provider.GetEncoder();
        return converter.Length is 0
            ? new FallbackVariableEncoder<E>(converter)
            : new FallbackConstantEncoder<E>(converter);
    }

    internal static SpanLikeDecoder<T>? GetDecoderOrDefault<T, E>(Converter<E> converter)
    {
        if (converter is ISpanLikeDecoderProvider<T> provider)
            return provider.GetDecoder();
        return null;
    }
}
