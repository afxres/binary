namespace Mikodev.Binary.Internal.SpanLike.Contexts;

internal interface ISpanLikeDecoderProvider<T>
{
    SpanLikeDecoder<T> GetDecoder();
}
