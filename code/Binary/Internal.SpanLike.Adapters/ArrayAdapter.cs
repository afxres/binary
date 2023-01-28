namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal readonly struct ArrayAdapter<E> : ISpanLikeAdapter<E[], E>
{
    public static ReadOnlySpan<E> AsSpan(E[]? item)
    {
        Debug.Assert(RuntimeHelpers.IsReferenceOrContainsReferences<E>() is false);
        return item;
    }

    public static int Length(E[]? item)
    {
        return item is null ? 0 : item.Length;
    }

    public static void Encode(ref Allocator allocator, E[]? item, Converter<E> converter)
    {
        if (item is null)
            return;
        for (var i = 0; i < item.Length; i++)
            converter.EncodeAuto(ref allocator, item[i]);
        return;
    }
}
