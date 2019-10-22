using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    [DebuggerStepThrough]
    internal static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowConverterLengthInvalid() => throw new ArgumentException("Converter length must be greater than or equal to zero!");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowAllocatorMaxCapacityInvalid() => throw new ArgumentException("Allocator max capacity must be greater than or equal to zero!");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowAllocatorOverflow() => throw new ArgumentException("Maximum allocator capacity has been reached.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowAllocatorModified() => throw new InvalidOperationException("Allocator has been modified unexpectedly!");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowTupleNull(Type type) => throw new ArgumentNullException("item", $"Tuple can not be null, type: {type}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNumberNegative() => throw new ArgumentException("Encode number can not be negative!");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowNotEnoughBytes() => throw new ArgumentException("Not enough bytes.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static T ThrowNotEnoughBytes<T>() => throw new ArgumentException($"Not enough bytes, type: {typeof(T)}");

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static T ThrowNoSuitableConstructor<T>() => throw new InvalidOperationException($"No suitable constructor found, type: {typeof(T)}");
    }
}
