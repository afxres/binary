namespace Mikodev.Binary.Internal.SpanLike.Builders;

using System;
using System.Collections.Generic;

internal sealed class ListBuilder<T> : SpanLikeBuilder<List<T>, T>
{
    public override ReadOnlySpan<T> Handle(List<T>? item)
    {
        return NativeModule.AsSpan(item);
    }

    public override List<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> invoke)
    {
        return invoke.Decode(span).GetList();
    }
}
