namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal readonly struct ArraySegmentAdapter<E> : ISpanLikeAdapter<ArraySegment<E>, E>
{
    public static ReadOnlySpan<E> AsSpan(ArraySegment<E> item)
    {
        Debug.Assert(RuntimeHelpers.IsReferenceOrContainsReferences<E>() is false);
        return item;
    }

    public static int Length(ArraySegment<E> item)
    {
        return item.Count;
    }

    public static void Encode(ref Allocator allocator, ArraySegment<E> item, Converter<E> converter)
    {
        var buffer = item.Array;
        var length = item.Count;
        if (buffer is null || length is 0)
            return;
        var offset = item.Offset;
        for (var i = offset; i < (offset + length); i++)
            converter.EncodeAuto(ref allocator, buffer[i]);
        return;
    }
}
