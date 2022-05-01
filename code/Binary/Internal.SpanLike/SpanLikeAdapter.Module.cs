namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Internal.SpanLike.Adapters;
using Mikodev.Binary.Internal.SpanLike.Contexts;

internal static class SpanLikeAdapter
{
    internal static SpanLikeAdapter<T> Create<T>(Converter<T> converter)
    {
        if (converter is ISpanLikeAdapterCreator<T> creator)
            return creator.GetAdapter();
        if (converter.Length > 0)
            return new ConstantAdapter<T>(converter);
        else
            return new VariableAdapter<T>(converter);
    }
}
