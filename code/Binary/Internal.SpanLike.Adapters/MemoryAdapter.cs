namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal readonly struct MemoryAdapter<E> : ISpanLikeAdapter<Memory<E>, E>
{
    public static ReadOnlySpan<E> AsSpan(Memory<E> item)
    {
        Debug.Assert(RuntimeHelpers.IsReferenceOrContainsReferences<E>() is false);
        return item.Span;
    }

    public static int Length(Memory<E> item)
    {
        return item.Length;
    }

    public static void Encode(ref Allocator allocator, Memory<E> item, Converter<E> converter)
    {
        foreach (var i in item.Span)
            converter.EncodeAuto(ref allocator, i);
        return;
    }
}
