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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        var offset = allocator.offset;
        Debug.Assert(offset >= 0);
        var limits = allocator.MaxCapacity;
        var amount = (long)(uint)offset + (uint)length;
        if (amount > limits)
            ThrowHelper.ThrowMaxCapacityOverflow();

        var source = allocator.bounds;
        var cursor = (long)source;
        Debug.Assert(cursor < amount);
        Debug.Assert(cursor < limits);
        const int Capacity = 128;
        if (cursor is 0)
            cursor = Capacity;
        do
            cursor *= 2;
        while (cursor < amount);
        if (cursor > limits)
            cursor = limits;
        Debug.Assert(amount <= cursor);
        Debug.Assert(cursor <= limits);

        var bounds = (int)cursor;
        var underlying = allocator.underlying;
        if (underlying is not null)
        {
            ref var target = ref underlying.Resize(bounds);
            allocator.target = ref target;
            allocator.bounds = bounds;
        }
        else
        {
            ref var target = ref MemoryMarshal.GetArrayDataReference(new byte[bounds]);
            if (offset is not 0)
                Unsafe.CopyBlockUnaligned(ref target, ref allocator.target, (uint)offset);
            allocator.target = ref target;
            allocator.bounds = bounds;
        }
        Debug.Assert(offset <= source);
        Debug.Assert(offset <= allocator.bounds);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Enough(ref Allocator allocator, int length)
    {
        Debug.Assert(allocator.bounds >= 0);
        Debug.Assert(allocator.offset >= 0);
        Debug.Assert(allocator.bounds >= allocator.offset);
        return (uint)allocator.bounds >= (ulong)(uint)allocator.offset + (uint)length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte Cursor(ref Allocator allocator)
    {
        Debug.Assert(allocator.bounds >= 0);
        Debug.Assert(allocator.offset >= 0);
        Debug.Assert(allocator.bounds >= allocator.offset);
        var offset = allocator.offset;
        return ref Unsafe.Add(ref allocator.target, offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref byte Create(ref Allocator allocator, int length)
    {
        Ensure(ref allocator, length);
        var offset = allocator.offset;
        return ref Unsafe.Add(ref allocator.target, offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FinishCreate(ref Allocator allocator, int length)
    {
        var offset = allocator.offset;
        Debug.Assert(length >= 0);
        Debug.Assert(offset >= 0);
        Debug.Assert(offset <= allocator.bounds);
        Debug.Assert(length <= allocator.bounds - offset);
        allocator.offset = offset + length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref byte Assign(ref Allocator allocator, int length)
    {
        Debug.Assert(length is not 0);
        Ensure(ref allocator, length);
        var offset = allocator.offset;
        allocator.offset = offset + length;
        return ref Unsafe.Add(ref allocator.target, offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Anchor(ref Allocator allocator)
    {
        Ensure(ref allocator, sizeof(int));
        var offset = allocator.offset;
        allocator.offset = offset + sizeof(int);
        return offset;
    }

    internal static void FinishAnchor(ref Allocator allocator, int anchor)
    {
        const int Limits = 16;
        var offset = allocator.offset;
        var result = (long)(uint)offset - (uint)anchor - sizeof(int);
        if (result < 0)
            ThrowHelper.ThrowInvalidAllocator();
        var length = (int)result;
        ref var target = ref Unsafe.Add(ref allocator.target, anchor);
        if (length <= Limits && allocator.bounds - offset >= ((-length) & 7))
        {
            allocator.offset = offset - 3;
            NumberModule.Encode(ref target, (uint)length, numberLength: 1);
            var header = (length + 7) >> 3;
            if (header is not 0)
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref target, 0 + 1), Unsafe.ReadUnaligned<long>(ref Unsafe.Add(ref target, 0 + 4)));
            if (header is 2)
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref target, 8 + 1), Unsafe.ReadUnaligned<long>(ref Unsafe.Add(ref target, 8 + 4)));
            Debug.Assert(allocator.offset >= 1);
            Debug.Assert(allocator.offset <= allocator.bounds);
        }
        else
        {
            NumberModule.Encode(ref target, (uint)length, numberLength: 4);
            Debug.Assert(allocator.offset >= 4);
            Debug.Assert(allocator.offset <= allocator.bounds);
        }
    }
}
