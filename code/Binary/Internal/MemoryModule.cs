namespace Mikodev.Binary.Internal;

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUM2 = System.Int16;
using NUM4 = System.Int32;
using NUM8 = System.Int64;

internal static class MemoryModule
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref byte EnsureLength(ReadOnlySpan<byte> span, int length)
    {
        Debug.Assert(length is not 0);
        if ((uint)span.Length < (uint)length)
            ThrowHelper.ThrowNotEnoughBytes();
        return ref MemoryMarshal.GetReference(span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref byte EnsureLength(ref ReadOnlySpan<byte> span, int length)
    {
        Debug.Assert(length is not 0);
        ref var source = ref MemoryMarshal.GetReference(span);
        var limits = span.Length;
        if ((uint)limits < (uint)length)
            ThrowHelper.ThrowNotEnoughBytes();
        span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, length), limits - length);
        return ref source;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T EnsureEndian<T>(T item, bool swap) where T : unmanaged
    {
        return item switch
        {
            NUM2 => swap is false ? item : (T)(object)BinaryPrimitives.ReverseEndianness((NUM2)(object)item),
            NUM4 => swap is false ? item : (T)(object)BinaryPrimitives.ReverseEndianness((NUM4)(object)item),
            NUM8 => swap is false ? item : (T)(object)BinaryPrimitives.ReverseEndianness((NUM8)(object)item),
            _ => throw new NotSupportedException(),
        };
    }
}
