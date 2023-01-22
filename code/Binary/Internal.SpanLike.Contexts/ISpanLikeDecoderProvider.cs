namespace Mikodev.Binary.Internal.SpanLike.Contexts;

internal interface ISpanLikeDecoderProvider<E>
{
    SpanLikeDecoder<E> GetDecoder();
}
