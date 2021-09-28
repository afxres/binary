namespace Mikodev.Binary.Internal;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class NativeModule
{
    private sealed class RawListData<T>
    {
        [AllowNull]
        public T[] Data;

        public int Size;
    }

    private struct RawImmutableArrayData<T>
    {
        [AllowNull]
        public T[] Data;
    }

    internal static ReadOnlySpan<T> AsSpan<T>(List<T>? source)
    {
        if (source is null || source.Count is 0)
            return default;
        var buffer = Unsafe.As<RawListData<T>>(source).Data;
        var length = Unsafe.As<RawListData<T>>(source).Size;
        return new ReadOnlySpan<T>(buffer, 0, length);
    }

    internal static ReadOnlySpan<T> AsSpan<T>(ImmutableArray<T> source)
    {
        // unsafe get array
        var buffer = Unsafe.As<ImmutableArray<T>, RawImmutableArrayData<T>>(ref source).Data;
        return new ReadOnlySpan<T>(buffer);
    }

    internal static List<T> CreateList<T>(T[] buffer, int length)
    {
        Debug.Assert((uint)length <= (uint)buffer.Length);
        var result = new List<T>();
        Unsafe.As<RawListData<T>>(result).Data = buffer;
        Unsafe.As<RawListData<T>>(result).Size = length;
        return result;
    }

    internal static ImmutableArray<T> CreateImmutableArray<T>(T[] buffer)
    {
        Debug.Assert(buffer is not null);
        var result = default(ImmutableArray<T>);
        Unsafe.As<ImmutableArray<T>, RawImmutableArrayData<T>>(ref result).Data = buffer;
        return result;
    }
}
