using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
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
            const int Initial = 64;
            if (cursor is 0)
                cursor = Initial;
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
        internal static void Ensure(ref Allocator allocator, int length)
        {
            if ((ulong)(uint)allocator.offset + (uint)length > (uint)allocator.buffer.Length)
                Resize(ref allocator, length);
            Debug.Assert(allocator.buffer.Length <= allocator.MaxCapacity);
            Debug.Assert(allocator.buffer.Length >= allocator.offset + length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Expand(ref Allocator allocator, int length)
        {
            Ensure(ref allocator, length);
            allocator.offset += length;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Append(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            var length = span.Length;
            if (length is 0)
                return;
            Unsafe.CopyBlockUnaligned(ref Assign(ref allocator, length), ref MemoryMarshal.GetReference(span), (uint)length);
        }

        internal static void AppendLengthPrefix(ref Allocator allocator, int anchor)
        {
            const int Limits = 16;
            var offset = allocator.offset;
            // check bounds manually
            if ((ulong)(uint)anchor + sizeof(uint) > (uint)offset)
                ThrowHelper.ThrowAllocatorOrAnchorInvalid();
            var length = offset - anchor - sizeof(uint);
            var buffer = allocator.buffer;
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), anchor);
            if (length <= Limits && buffer.Length - offset >= ((-length) & 7))
            {
                MemoryHelper.EncodeNumber(ref target, (uint)length, numberLength: 1);
                for (var i = 0; i < length; i += 8)
                    MemoryHelper.EncodeNativeEndian(ref Unsafe.Add(ref target, i + 1), MemoryHelper.DecodeNativeEndian<long>(ref Unsafe.Add(ref target, i + 4)));
                allocator.offset = offset - 3;
            }
            else
            {
                MemoryHelper.EncodeNumber(ref target, (uint)length, numberLength: 4);
            }
        }
    }
}
