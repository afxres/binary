namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using Mikodev.Binary.Internal.SpanLike.Encoders;

internal static class SpanLikeContext
{
    public static SpanLikeEncoder<E> GetEncoder<E>(Converter<E> converter)
    {
        if (converter is ISpanLikeEncoderProvider<E> provider)
            return provider.GetEncoder();
        return converter.Length is 0
            ? new FallbackVariableEncoder<E>(converter)
            : new FallbackConstantEncoder<E>(converter);
    }

    public static SpanLikeDecoder<E>? GetDecoderOrDefault<E>(Converter<E> converter)
    {
        if (converter is ISpanLikeDecoderProvider<E> provider)
            return provider.GetDecoder();
        return null;
    }
}
