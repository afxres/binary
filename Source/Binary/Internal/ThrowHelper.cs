﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    [DebuggerStepThrough]
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentLengthInvalid() => throw new ArgumentOutOfRangeException("length", "Argument length must be greater than or equal to zero!");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentNumberInvalid() => throw new ArgumentOutOfRangeException("number", "Argument number must be greater than or equal to zero!");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowAllocatorMaxCapacityInvalid() => throw new ArgumentException("Maximum allocator capacity must be greater than or equal to zero!");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowAllocatorOverflow() => throw new ArgumentException("Maximum allocator capacity has been reached.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowLengthPrefixAnchorInvalid() => throw new ArgumentException("Invalid allocator anchor for length prefix.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowAllocatorActionInvalid() => throw new ArgumentNullException("action");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowTupleNull(Type type) => throw new ArgumentNullException("item", $"Tuple can not be null, type: {type}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowCollectionBytesInvalid(Type type, int byteCount, int remainder) => throw new ArgumentException($"Invalid collection bytes, byte count: {byteCount}, remainder: {remainder}, item type: {type}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotEnoughBytes() => throw new ArgumentException("Not enough bytes.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static ReadOnlySpan<T> ThrowNotEnoughBytesReadOnlySpan<T>() => throw new ArgumentException($"Not enough bytes, type: {typeof(ReadOnlySpan<T>)}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static T ThrowNotEnoughBytes<T>() => throw new ArgumentException($"Not enough bytes, type: {typeof(T)}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static T ThrowNoSuitableConstructor<T>() => throw new InvalidOperationException($"No suitable constructor found, type: {typeof(T)}");
    }
}
