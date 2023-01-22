namespace Mikodev.Binary.Internal.SpanLike.Contexts;

internal interface ISpanLikeEncoderProvider<E>
{
    SpanLikeEncoder<E> GetEncoder();
}
