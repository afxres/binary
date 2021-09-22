namespace Mikodev.Binary.Internal;

using System;
using System.Collections.Generic;
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

    internal static ReadOnlySpan<T> AsSpan<T>(List<T>? list)
    {
        if (list is null || list.Count is 0)
            return default;
        var buffer = Unsafe.As<RawListData<T>>(list).Data;
        var length = Unsafe.As<RawListData<T>>(list).Size;
        return new ReadOnlySpan<T>(buffer, 0, length);
    }

    internal static List<T> CreateList<T>(T[] buffer, int length)
    {
        Debug.Assert((uint)length <= (uint)buffer.Length);
        var list = new List<T>();
        Unsafe.As<RawListData<T>>(list).Data = buffer;
        Unsafe.As<RawListData<T>>(list).Size = length;
        return list;
    }
}
