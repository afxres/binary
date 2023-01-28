﻿namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal readonly struct ListAdapter<E> : ISpanLikeAdapter<List<E>, E>
{
    public static ReadOnlySpan<E> AsSpan(List<E>? item)
    {
        Debug.Assert(RuntimeHelpers.IsReferenceOrContainsReferences<E>() is false);
        return CollectionsMarshal.AsSpan(item);
    }

    public static int Length(List<E>? item)
    {
        return item is null ? 0 : item.Count;
    }

    public static void Encode(ref Allocator allocator, List<E>? item, Converter<E> converter)
    {
        if (item is null)
            return;
        for (var i = 0; i < item.Count; i++)
            converter.EncodeAuto(ref allocator, item[i]);
        return;
    }
}