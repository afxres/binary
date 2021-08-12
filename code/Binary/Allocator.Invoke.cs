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
        Debug.Assert(cursor <= limits);
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

        var target = new Span<byte>(new byte[(int)cursor]);
        Debug.Assert(offset <= source.Length);
        Debug.Assert(offset <= target.Length);
        if (offset is not 0)
            Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(target), ref MemoryMarshal.GetReference(source), (uint)offset);
        allocator.buffer = target;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Anchor(ref Allocator allocator, int length)
    {
        Ensure(ref allocator, length);
        var offset = allocator.offset;
        allocator.offset = offset + length;
        return offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref byte Assign(ref Allocator allocator, int length)
    {
        Debug.Assert(length is not 0);
        var offset = Anchor(ref allocator, length);
        var buffer = allocator.buffer;
        return ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
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
            NumberHelper.Encode(ref target, (uint)length, numberLength: 1);
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
            NumberHelper.Encode(ref target, (uint)length, numberLength: 4);
            Debug.Assert(allocator.offset >= 4);
            Debug.Assert(allocator.offset <= allocator.buffer.Length);
        }
    }
}
