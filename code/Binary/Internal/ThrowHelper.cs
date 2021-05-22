using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Mikodev.Binary.Internal
{
    [DebuggerStepThrough]
    internal static class ThrowHelper
    {
        [DoesNotReturn]
        internal static void ThrowActionNull() => throw new ArgumentNullException("action");

        [DoesNotReturn]
        internal static void ThrowEncodingNull() => throw new ArgumentNullException("encoding");

        [DoesNotReturn]
        internal static void ThrowTypeNull() => throw new ArgumentNullException("type");

        [DoesNotReturn]
        internal static void ThrowAllocatorInvalid() => throw new InvalidOperationException("Allocator has been modified unexpectedly!");

        [DoesNotReturn]
        internal static void ThrowLengthNegative() => throw new ArgumentOutOfRangeException("length", "Argument length must be greater than or equal to zero!");

        [DoesNotReturn]
        internal static void ThrowNumberNegative() => throw new ArgumentOutOfRangeException("number", "Argument number must be greater than or equal to zero!");

        [DoesNotReturn]
        internal static void ThrowMaxCapacityNegative() => throw new ArgumentOutOfRangeException("maxCapacity", "Maximum capacity must be greater than or equal to zero!");

        [DoesNotReturn]
        internal static void ThrowMaxCapacityOverflow() => throw new ArgumentException("Maximum capacity has been reached.");

        [DoesNotReturn]
        internal static void ThrowTupleNull<T>() => throw new ArgumentNullException("item", $"Tuple can not be null, type: {typeof(T)}");

        [DoesNotReturn]
        internal static void ThrowNotEnoughBytesCollection<T>(int byteLength) => throw new ArgumentException($"Not enough bytes for collection element, byte length: {byteLength}, element type: {typeof(T)}");

        [DoesNotReturn]
        internal static void ThrowNotEnoughBytes() => throw new ArgumentException("Not enough bytes or byte sequence invalid.");

        [DoesNotReturn]
        internal static Type ThrowNotConverter(Type type) => throw new ArgumentException($"Can not get generic argument, '{type}' is not a subclass of '{typeof(Converter<>)}'");

        [DoesNotReturn]
        internal static T ThrowNoSuitableConstructor<T>() => throw new NotSupportedException($"No suitable constructor found, type: {typeof(T)}");

        [DoesNotReturn]
        internal static T? ThrowNullableTagInvalid<T>(int tag) where T : struct => throw new ArgumentException($"Invalid nullable tag '{tag}', type: {typeof(T?)}");
    }
}
