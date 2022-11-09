namespace Mikodev.Binary.Internal;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class NativeModule
{
    private sealed class RawBitArrayData
    {
        [AllowNull]
#pragma warning disable CS0649 // Field 'NativeModule.RawBitArrayData.Data' is never assigned to, and will always have its default value null
        public int[] Data;
#pragma warning restore CS0649 // Field 'NativeModule.RawBitArrayData.Data' is never assigned to, and will always have its default value null
    }

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

    internal static Span<int> AsSpan(BitArray source)
    {
        Debug.Assert(source is not null);
        var buffer = Unsafe.As<RawBitArrayData>(source).Data;
        return new Span<int>(buffer);
    }

    internal static ReadOnlySpan<T> AsReadOnlySpan<T>(List<T>? source)
    {
        if (source is null || source.Count is 0)
            return default;
        var buffer = Unsafe.As<RawListData<T>>(source).Data;
        var length = Unsafe.As<RawListData<T>>(source).Size;
        return new ReadOnlySpan<T>(buffer, 0, length);
    }

    internal static ReadOnlySpan<T> AsReadOnlySpan<T>(ImmutableArray<T> source)
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
