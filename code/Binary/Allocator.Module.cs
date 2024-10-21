namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public ref partial struct Allocator
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Append(ref Allocator allocator, byte data)
    {
        Unsafe.WriteUnaligned(ref Assign(ref allocator, sizeof(byte)), data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Append(ref Allocator allocator, scoped ReadOnlySpan<byte> span)
    {
        var length = span.Length;
        if (length is 0)
            return;
        Unsafe.CopyBlockUnaligned(ref Assign(ref allocator, length), ref MemoryMarshal.GetReference(span), (uint)length);
    }

#if NET9_0_OR_GREATER
    public static void Append<T>(ref Allocator allocator, int length, T data, SpanAction<byte, T> action) where T : allows ref struct
#else
    public static void Append<T>(ref Allocator allocator, int length, T data, SpanAction<byte, T> action)
#endif
    {
        ArgumentNullException.ThrowIfNull(action);
        if (length is 0)
            return;
        action.Invoke(MemoryMarshal.CreateSpan(ref Assign(ref allocator, length), length), data);
    }

#if NET9_0_OR_GREATER
    public static void Append<T>(ref Allocator allocator, int maxLength, T data, AllocatorWriter<T> writer) where T : allows ref struct
#else
    public static void Append<T>(ref Allocator allocator, int maxLength, T data, AllocatorWriter<T> writer)
#endif
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        if (maxLength is 0)
            return;
        ref var target = ref Create(ref allocator, maxLength);
        var actual = writer.Invoke(MemoryMarshal.CreateSpan(ref target, maxLength), data);
        if ((uint)actual > (uint)maxLength)
            ThrowHelper.ThrowInvalidReturnValue();
        Debug.Assert(actual >= 0);
        Debug.Assert(actual <= maxLength);
        FinishCreate(ref allocator, actual);
    }

#if NET9_0_OR_GREATER
    public static void AppendWithLengthPrefix<T>(ref Allocator allocator, int maxLength, T data, AllocatorWriter<T> writer) where T : allows ref struct
#else
    public static void AppendWithLengthPrefix<T>(ref Allocator allocator, int maxLength, T data, AllocatorWriter<T> writer)
#endif
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);
        var numberLength = NumberModule.EncodeLength((uint)maxLength);
        ref var target = ref Create(ref allocator, maxLength + numberLength);
        var actual = maxLength is 0 ? 0 : writer.Invoke(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref target, numberLength), maxLength), data);
        if ((uint)actual > (uint)maxLength)
            ThrowHelper.ThrowInvalidReturnValue();
        Debug.Assert(actual >= 0);
        Debug.Assert(actual <= maxLength);
        NumberModule.Encode(ref target, (uint)actual, numberLength);
        FinishCreate(ref allocator, actual + numberLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NET9_0_OR_GREATER
    public static void AppendWithLengthPrefix<T>(ref Allocator allocator, T data, AllocatorAction<T> action) where T : allows ref struct
#else
    public static void AppendWithLengthPrefix<T>(ref Allocator allocator, T data, AllocatorAction<T> action)
#endif
    {
        ArgumentNullException.ThrowIfNull(action);
        var anchor = Anchor(ref allocator);
        action.Invoke(ref allocator, data);
        FinishAnchor(ref allocator, anchor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ensure(ref Allocator allocator, int length)
    {
        if (Enough(ref allocator, length) is false)
            Resize(ref allocator, length);
        Debug.Assert(allocator.bounds <= allocator.MaxCapacity);
        Debug.Assert(allocator.bounds >= allocator.offset + length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Expand(ref Allocator allocator, int length)
    {
        Ensure(ref allocator, length);
        allocator.offset += length;
    }

#if NET9_0_OR_GREATER
    public static byte[] Invoke<T>(T data, AllocatorAction<T> action) where T : allows ref struct
#else
    public static byte[] Invoke<T>(T data, AllocatorAction<T> action)
#endif
    {
        ArgumentNullException.ThrowIfNull(action);
        var handle = BufferModule.Borrow();
        try
        {
            var allocator = new Allocator(BufferModule.Intent(handle));
            action.Invoke(ref allocator, data);
            return allocator.ToArray();
        }
        finally
        {
            BufferModule.Return(handle);
        }
    }
}
