namespace Mikodev.Binary.Internal;

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Int16 = System.Int16;
using Int32 = System.Int32;
using Int64 = System.Int64;

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
            Int16 => swap is false ? item : (T)(object)BinaryPrimitives.ReverseEndianness((Int16)(object)item),
            Int32 => swap is false ? item : (T)(object)BinaryPrimitives.ReverseEndianness((Int32)(object)item),
            Int64 => swap is false ? item : (T)(object)BinaryPrimitives.ReverseEndianness((Int64)(object)item),
            _ => throw new NotSupportedException(),
        };
    }
}
