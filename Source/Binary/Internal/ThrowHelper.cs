using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    [DebuggerStepThrough]
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentNull(string paramName) => throw new ArgumentNullException(paramName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowArgumentOutOfRange(string paramName) => throw new ArgumentOutOfRangeException(paramName);

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowAllocatorOverflow() => throw new ArgumentException("Maximum allocator capacity has been reached.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowAllocatorModified() => throw new InvalidOperationException("Allocator has been modified unexpectedly!");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowInvalidNullableTag(int tag, Type type) => throw new ArgumentException($"Invalid nullable tag: {tag}, type: {type}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowTupleNull(Type type) => throw new ArgumentNullException("item", $"Tuple can not be null, type: {type}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotEnoughBytes() => throw new ArgumentException("Not enough bytes.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNumberOverflow() => throw new ArgumentException("Encode number overflow.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNumberInvalidBytes() => throw new ArgumentException("Decode number bytes invalid.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static T ThrowNotEnoughBytes<T>() => throw new ArgumentException($"Not enough bytes, type: {typeof(T)}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static T ThrowNoSuitableConstructor<T>() => throw new InvalidOperationException($"No suitable constructor found, type: {typeof(T)}");
    }
}
