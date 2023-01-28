namespace Mikodev.Binary.Internal.SpanLike.Contexts;

internal interface ISpanLikeEncoderProvider<E>
{
    SpanLikeForwardEncoder<E> GetEncoder();
}
