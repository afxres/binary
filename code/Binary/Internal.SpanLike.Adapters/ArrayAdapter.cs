namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;

internal readonly struct ArrayAdapter<E> : ISpanLikeAdapter<E[], E>
{
    public static ReadOnlySpan<E> AsSpan(E[]? item)
    {
        return item;
    }

    public static E[] Invoke(E[] values, int length)
    {
        if (values.Length != length)
            Array.Resize(ref values, length);
        return values;
    }
}
