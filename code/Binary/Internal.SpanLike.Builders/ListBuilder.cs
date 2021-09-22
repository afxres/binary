namespace Mikodev.Binary.Internal.SpanLike.Builders;

using System;
using System.Collections.Generic;
using System.Diagnostics;

internal sealed class ListBuilder<T> : SpanLikeBuilder<List<T>, T>
{
    public override ReadOnlySpan<T> Handle(List<T>? item)
    {
        return NativeModule.AsSpan(item);
    }

    public override List<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> invoke)
    {
        var (buffer, length) = invoke.Decode(span);
        Debug.Assert((uint)length <= (uint)buffer.Length);
        return NativeModule.CreateList(buffer, length);
    }
}
