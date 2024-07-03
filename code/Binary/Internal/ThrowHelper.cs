namespace Mikodev.Binary.Internal;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

[DebuggerStepThrough]
internal static class ThrowHelper
{
    [DoesNotReturn]
    internal static void ThrowAllocatorInvalid() => throw new InvalidOperationException("Allocator has been modified unexpectedly!");

    [DoesNotReturn]
    internal static void ThrowMaxCapacityOverflow() => throw new ArgumentException("Maximum capacity has been reached.");

    [DoesNotReturn]
    internal static void ThrowInvalidReturnValue() => throw new InvalidOperationException("Invalid return value.");

    [DoesNotReturn]
    internal static void ThrowTupleNull<T>() => throw new ArgumentException($"Tuple can not be null, type: {typeof(T)}");

    [DoesNotReturn]
    internal static void ThrowNotEnoughBytesCollection<T>(int byteLength) => throw new ArgumentException($"Not enough bytes for collection element, byte length: {byteLength}, element type: {typeof(T)}");

    [DoesNotReturn]
    internal static void ThrowNotEnoughBytes() => throw new ArgumentException("Not enough bytes or byte sequence invalid.");

    [DoesNotReturn]
    internal static void ThrowNotEnoughBytesToWrite() => throw new ArgumentException("Not enough bytes to write.");

    [DoesNotReturn]
    internal static void ThrowTryWriteBytesFailed() => throw new InvalidOperationException("Try write bytes failed.");

    [DoesNotReturn]
    internal static void ThrowNotOverride(string auto, string prefix, Type type) => throw new InvalidOperationException($"Method '{auto}' should be overridden if method '{prefix}' has been overridden, type: {type}");

    [DoesNotReturn]
    internal static void ThrowAmbiguousMembers(string memberName, Type type) => throw new ArgumentException($"Get members error, ambiguous members detected, member name: {memberName}, type: {type}");

    [DoesNotReturn]
    internal static void ThrowNotConverter(Type type) => throw new ArgumentException($"Invalid converter instance, '{type}' is not a subclass of '{typeof(Converter<>)}'");

    [DoesNotReturn]
    internal static void ThrowNoSuitableConstructor<T>() => throw new NotSupportedException($"No suitable constructor found, type: {typeof(T)}");

    [DoesNotReturn]
    internal static void ThrowNullableTagInvalid<T>(int tag) where T : struct => throw new ArgumentException($"Invalid nullable tag '{tag}', type: {typeof(T?)}");
}
