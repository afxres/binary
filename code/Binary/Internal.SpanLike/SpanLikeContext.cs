namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Internal.SpanLike.Contexts;

internal static class SpanLikeContext
{
    internal static SpanLikeForwardEncoder<E>? GetEncoderOrDefault<E>(Converter<E> converter)
    {
        if (converter is ISpanLikeEncoderProvider<E> provider)
            return provider.GetEncoder();
        return null;
    }

    internal static SpanLikeDecoder<T>? GetDecoderOrDefault<T, E>(Converter<E> converter)
    {
        if (converter is ISpanLikeDecoderProvider<T> provider)
            return provider.GetDecoder();
        return null;
    }
}
