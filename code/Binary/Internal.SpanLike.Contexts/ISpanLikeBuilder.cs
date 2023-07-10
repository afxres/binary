namespace Mikodev.Binary.Internal.SpanLike.Contexts;

internal interface ISpanLikeBuilder<out T, in E>
{
    static abstract T Invoke(E[] array, int count);
}
