namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

internal readonly struct ImmutableArrayAdapter<E> : ISpanLikeAdapter<ImmutableArray<E>, E>
{
    public static ReadOnlySpan<E> AsSpan(ImmutableArray<E> item)
    {
        return item.AsSpan();
    }

    public static ImmutableArray<E> Invoke(E[] values, int length)
    {
        if (values.Length != length)
            Array.Resize(ref values, length);
        return ImmutableCollectionsMarshal.AsImmutableArray(values);
    }
}
