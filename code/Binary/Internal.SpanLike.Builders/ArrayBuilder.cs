namespace Mikodev.Binary.Internal.SpanLike.Builders;

using System;

internal sealed class ArrayBuilder<T> : SpanLikeBuilder<T[], T>
{
    public override ReadOnlySpan<T> Handle(T[]? item)
    {
        return item;
    }

    public override T[] Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> invoke)
    {
        return invoke.Decode(span).GetArray();
    }
}
