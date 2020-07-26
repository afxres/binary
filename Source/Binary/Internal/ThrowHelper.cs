using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Mikodev.Binary.Internal
{
    [DebuggerStepThrough]
    internal static class ThrowHelper
    {
        [DoesNotReturn]
        internal static void ThrowArgumentLengthOutOfRange() => throw new ArgumentOutOfRangeException("length", "Argument length must be greater than or equal to zero!");

        [DoesNotReturn]
        internal static void ThrowArgumentNumberOutOfRange() => throw new ArgumentOutOfRangeException("number", "Argument number must be greater than or equal to zero!");

        [DoesNotReturn]
        internal static void ThrowArgumentEncodingNull() => throw new ArgumentNullException("encoding");

        [DoesNotReturn]
        internal static void ThrowAllocatorMaxCapacityOutOfRange() => throw new ArgumentOutOfRangeException("maxCapacity", "Maximum allocator capacity must be greater than or equal to zero!");

        [DoesNotReturn]
        internal static void ThrowAllocatorOverflow() => throw new ArgumentException("Maximum allocator capacity has been reached.");

        [DoesNotReturn]
        internal static void ThrowAllocatorAnchorOutOfRange() => throw new ArgumentOutOfRangeException("anchor");

        [DoesNotReturn]
        internal static void ThrowAllocatorActionNull() => throw new ArgumentNullException("action");

        [DoesNotReturn]
        internal static void ThrowTupleNull(Type type) => throw new ArgumentNullException("item", $"Tuple can not be null, type: {type}");

        [DoesNotReturn]
        internal static void ThrowCollectionBytesInvalid(Type type, int bytes, int remainder) => throw new ArgumentException($"Invalid collection bytes, byte count: {bytes}, remainder: {remainder}, item type: {type}");

        [DoesNotReturn]
        internal static void ThrowNotEnoughBytes() => throw new ArgumentException("Not enough bytes or byte sequence invalid.");

        [DoesNotReturn]
        internal static ReadOnlySpan<T> ThrowNotEnoughBytesReadOnlySpan<T>() => throw new ArgumentException("Not enough bytes or byte sequence invalid.");

        [DoesNotReturn]
        internal static T ThrowNotEnoughBytes<T>() => throw new ArgumentException("Not enough bytes or byte sequence invalid.");

        [DoesNotReturn]
        internal static T ThrowNoSuitableConstructor<T>() => throw new NotSupportedException($"No suitable constructor found, type: {typeof(T)}");
    }
}
