namespace Mikodev.Binary.Internal.SpanLike.Builders;

using System;
using System.Diagnostics;

internal sealed class ReadOnlyMemoryBuilder<T> : SpanLikeBuilder<ReadOnlyMemory<T>, T>
{
    public override ReadOnlySpan<T> Handle(ReadOnlyMemory<T> item) => item.Span;

    public override ReadOnlyMemory<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> invoke)
    {
        var (buffer, length) = invoke.Decode(span);
        Debug.Assert((uint)length <= (uint)buffer.Length);
        return new ReadOnlyMemory<T>(buffer, 0, length);
    }
}
