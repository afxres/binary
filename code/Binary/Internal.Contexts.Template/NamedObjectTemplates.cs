namespace Mikodev.Binary.Internal.Contexts.Template;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static class NamedObjectTemplates
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long GetIndexData(int offset, int length)
    {
        return (long)(((ulong)(uint)offset << 32) | (uint)length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasIndexData(ReadOnlySpan<long> data, int index)
    {
        return data[index] is not 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlySpan<byte> GetIndexSpan(ReadOnlySpan<byte> span, ReadOnlySpan<long> data, int index)
    {
        Debug.Assert(span.Length is not 0);
        Debug.Assert(data.Length is not 0);
        var item = data[index];
        var body = span.Slice((int)(item >> 32), (int)item);
        return body;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool NotDefaultValue<T>(T? item)
    {
        return EqualityComparer<T>.Default.Equals(item, default) is false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Append(ref Allocator allocator, byte[] data)
    {
        Debug.Assert(data is not null);
        Debug.Assert(data.Length is not 0);
        Unsafe.CopyBlockUnaligned(ref Allocator.Assign(ref allocator, data.Length), ref MemoryMarshal.GetArrayDataReference(data), (uint)data.Length);
    }
}
