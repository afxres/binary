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
        if (action is null)
            ThrowHelper.ThrowActionNull();
        if (length is 0)
            return;
        action.Invoke(MemoryMarshal.CreateSpan(ref Assign(ref allocator, length), length), data);
    }

    public static void AppendWithLengthPrefix<T>(ref Allocator allocator, T data, AllocatorAction<T> action)
    {
        if (action is null)
            ThrowHelper.ThrowActionNull();
        var anchor = Anchor(ref allocator, sizeof(int));
        action.Invoke(ref allocator, data);
        FinishAnchor(ref allocator, anchor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ensure(ref Allocator allocator, int length)
    {
        if ((ulong)(uint)allocator.offset + (uint)length > (uint)allocator.buffer.Length)
            Resize(ref allocator, length);
        Debug.Assert(allocator.buffer.Length <= allocator.MaxCapacity);
        Debug.Assert(allocator.buffer.Length >= allocator.offset + length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Expand(ref Allocator allocator, int length)
    {
        Ensure(ref allocator, length);
        allocator.offset += length;
    }

    public static byte[] Invoke<T>(T data, AllocatorAction<T> action)
    {
        if (action is null)
            ThrowHelper.ThrowActionNull();
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
