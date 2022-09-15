namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public ref partial struct Allocator
{
    private static void Resize(ref Allocator allocator, int length)
    {
        if (length <= 0)
            ThrowHelper.ThrowLengthNegative();
        var offset = allocator.offset;
        Debug.Assert(offset >= 0);
        var limits = allocator.MaxCapacity;
        var amount = (long)(uint)offset + (uint)length;
        if (amount > limits)
            ThrowHelper.ThrowMaxCapacityOverflow();

        var source = allocator.buffer;
        var cursor = (long)source.Length;
        Debug.Assert(cursor < amount);
        Debug.Assert(cursor < limits);
        const int Capacity = 64;
        if (cursor is 0)
            cursor = Capacity;
        do
            cursor <<= 2;
        while (cursor < amount);
        if (cursor > limits)
            cursor = limits;
        Debug.Assert(amount <= cursor);
        Debug.Assert(cursor <= limits);

        var bounds = (int)cursor;
        var underlying = allocator.underlying;
        if (underlying is not null)
        {
            ref var values = ref underlying.Allocate(bounds);
            var target = MemoryMarshal.CreateSpan(ref values, bounds);
            allocator.buffer = target;
        }
        else
        {
            var target = new Span<byte>(new byte[bounds]);
            if (offset is not 0)
                Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(target), ref MemoryMarshal.GetReference(source), (uint)offset);
            allocator.buffer = target;
        }
        Debug.Assert(offset <= source.Length);
        Debug.Assert(offset <= allocator.buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte Create(ref Allocator allocator, int length)
    {
        Ensure(ref allocator, length);
        var offset = allocator.offset;
        var buffer = allocator.buffer;
        return ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FinishCreate(ref Allocator allocator, int length)
    {
        var offset = allocator.offset;
        Debug.Assert(length >= 0);
        Debug.Assert(offset >= 0);
        Debug.Assert(offset <= allocator.buffer.Length);
        Debug.Assert(length <= allocator.buffer.Length - offset);
        allocator.offset = offset + length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref byte Assign(ref Allocator allocator, int length)
    {
        Debug.Assert(length is not 0);
        Ensure(ref allocator, length);
        var offset = allocator.offset;
        allocator.offset = offset + length;
        var buffer = allocator.buffer;
        return ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Anchor(ref Allocator allocator, int length)
    {
        Ensure(ref allocator, length);
        var offset = allocator.offset;
        allocator.offset = offset + length;
        return offset;
    }

    internal static void FinishAnchor(ref Allocator allocator, int anchor)
    {
        const int Limits = 16;
        var offset = allocator.offset;
        var result = (long)(uint)offset - (uint)anchor - sizeof(int);
        if (result < 0)
            ThrowHelper.ThrowAllocatorInvalid();
        var length = (int)result;
        var buffer = allocator.buffer;
        ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), anchor);
        if (length <= Limits && buffer.Length - offset >= ((-length) & 7))
        {
            allocator.offset = offset - 3;
            NumberModule.Encode(ref target, (uint)length, numberLength: 1);
            var header = (length + 7) >> 3;
            if (header is not 0)
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref target, 0 + 1), Unsafe.ReadUnaligned<long>(ref Unsafe.Add(ref target, 0 + 4)));
            if (header is 2)
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref target, 8 + 1), Unsafe.ReadUnaligned<long>(ref Unsafe.Add(ref target, 8 + 4)));
            Debug.Assert(allocator.offset >= 1);
            Debug.Assert(allocator.offset <= allocator.buffer.Length);
        }
        else
        {
            NumberModule.Encode(ref target, (uint)length, numberLength: 4);
            Debug.Assert(allocator.offset >= 4);
            Debug.Assert(allocator.offset <= allocator.buffer.Length);
        }
    }
}
