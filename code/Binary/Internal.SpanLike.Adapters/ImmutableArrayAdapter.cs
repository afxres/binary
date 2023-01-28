namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal readonly struct ImmutableArrayAdapter<E> : ISpanLikeAdapter<ImmutableArray<E>, E>
{
    public static ReadOnlySpan<E> AsSpan(ImmutableArray<E> item)
    {
        Debug.Assert(RuntimeHelpers.IsReferenceOrContainsReferences<E>() is false);
        return item.AsSpan();
    }

    public static int Length(ImmutableArray<E> item)
    {
        return item.IsDefaultOrEmpty ? 0 : item.Length;
    }

    public static void Encode(ref Allocator allocator, ImmutableArray<E> item, Converter<E> converter)
    {
        if (item.IsDefaultOrEmpty)
            return;
        for (var i = 0; i < item.Length; i++)
            converter.EncodeAuto(ref allocator, item[i]);
        return;
    }
}
