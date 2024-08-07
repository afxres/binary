﻿namespace Mikodev.Binary.Internal;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
}
