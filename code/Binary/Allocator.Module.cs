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
    public static void Append(ref Allocator allocator, ReadOnlySpan<byte> span)
    {
        var length = span.Length;
        if (length is 0)
            return;
        Unsafe.CopyBlockUnaligned(ref Assign(ref allocator, length), ref MemoryMarshal.GetReference(span), (uint)length);
    }

    public static void Append<T>(ref Allocator allocator, int length, T data, SpanAction<byte, T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (length is 0)
            return;
        action.Invoke(MemoryMarshal.CreateSpan(ref Assign(ref allocator, length), length), data);
    }

    public static void Append<T>(ref Allocator allocator, int maxLength, T data, AllocatorWriter<T> writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (maxLength < 0)
            ThrowHelper.ThrowMaxLengthNegative();
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

    public static void AppendWithLengthPrefix<T>(ref Allocator allocator, int maxLength, T data, AllocatorWriter<T> writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (maxLength < 0)
            ThrowHelper.ThrowMaxLengthNegative();
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

    public static void AppendWithLengthPrefix<T>(ref Allocator allocator, T data, AllocatorAction<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var anchor = Anchor(ref allocator, sizeof(int));
        action.Invoke(ref allocator, data);
        FinishAnchor(ref allocator, anchor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ensure(ref Allocator allocator, int length)
    {
#if NET7_0_OR_GREATER
        if ((ulong)(uint)allocator.offset + (uint)length > (uint)allocator.bounds)
            Resize(ref allocator, length);
        Debug.Assert(allocator.bounds <= allocator.MaxCapacity);
        Debug.Assert(allocator.bounds >= allocator.offset + length);
#else
        if ((ulong)(uint)allocator.offset + (uint)length > (uint)allocator.buffer.Length)
            Resize(ref allocator, length);
        Debug.Assert(allocator.buffer.Length <= allocator.MaxCapacity);
        Debug.Assert(allocator.buffer.Length >= allocator.offset + length);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Expand(ref Allocator allocator, int length)
    {
        Ensure(ref allocator, length);
        allocator.offset += length;
    }

    public static byte[] Invoke<T>(T data, AllocatorAction<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var handle = BufferModule.Borrow();
        try
        {
            var allocator = new Allocator(BufferModule.Result(handle));
            action.Invoke(ref allocator, data);
            return allocator.ToArray();
        }
        finally
        {
            BufferModule.Return(handle);
        }
    }
}
