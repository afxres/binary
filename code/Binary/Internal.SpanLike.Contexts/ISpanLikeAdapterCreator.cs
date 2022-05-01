namespace Mikodev.Binary.Internal.SpanLike.Contexts;

internal interface ISpanLikeAdapterCreator<T>
{
    SpanLikeAdapter<T> GetAdapter();
}
