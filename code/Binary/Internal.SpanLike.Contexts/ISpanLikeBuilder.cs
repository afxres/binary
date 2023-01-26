namespace Mikodev.Binary.Internal.SpanLike.Contexts;

internal interface ISpanLikeBuilder<T, E>
{
    static abstract T Invoke(E[] array, int count);
}
