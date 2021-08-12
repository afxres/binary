namespace Mikodev.Binary.Internal.SpanLike.Builders;

using System;
using System.Diagnostics;

internal sealed class ArrayBuilder<T> : SpanLikeBuilder<T[], T>
{
    public override ReadOnlySpan<T> Handle(T[] item) => item;

    public override T[] Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> invoke)
    {
        var (buffer, length) = invoke.Decode(span);
        Debug.Assert((uint)length <= (uint)buffer.Length);
        if (buffer.Length == length)
            return buffer;
        var target = new T[length];
        Array.Copy(buffer, 0, target, 0, length);
        return target;
    }
}
