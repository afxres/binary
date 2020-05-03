using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        private static void Expand(ref Allocator allocator, int expand)
        {
            if (expand <= 0)
                ThrowHelper.ThrowArgumentLengthInvalid();
            var offset = allocator.offset;
            Debug.Assert(offset >= 0);
            var limits = allocator.MaxCapacity;
            var amount = (long)(uint)offset + (uint)expand;
            if (amount > limits)
                ThrowHelper.ThrowAllocatorOverflow();

            var source = allocator.buffer;
            var length = (long)source.Length;
            Debug.Assert(length < amount);
            Debug.Assert(length <= limits);
            const int Initial = 64;
            if (length == 0)
                length = Initial;
            do
                length <<= 2;
            while (length < amount);
            if (length > limits)
                length = limits;
            Debug.Assert(amount <= length);
            Debug.Assert(length <= limits);

            var target = new Span<byte>(new byte[(int)length]);
            Debug.Assert(offset <= source.Length);
            Debug.Assert(offset <= target.Length);
            if (offset != 0)
                Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(target), ref MemoryMarshal.GetReference(source), (uint)offset);
            allocator.buffer = target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Ensure(ref Allocator allocator, int length)
        {
            var offset = allocator.offset;
            if ((ulong)(uint)offset + (uint)length > (uint)allocator.buffer.Length)
                Expand(ref allocator, length);
            Debug.Assert(allocator.offset == offset);
            Debug.Assert(allocator.buffer.Length <= allocator.MaxCapacity);
            Debug.Assert(allocator.buffer.Length >= offset + length);
            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Anchor(ref Allocator allocator, int length)
        {
            var offset = Ensure(ref allocator, length);
            allocator.offset = offset + length;
            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref byte Assign(ref Allocator allocator, int length)
        {
            Debug.Assert(length > 0);
            var offset = Anchor(ref allocator, length);
            var buffer = allocator.buffer;
            return ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AppendBuffer(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            var length = span.Length;
            if (length == 0)
                return;
            Unsafe.CopyBlockUnaligned(ref Assign(ref allocator, length), ref MemoryMarshal.GetReference(span), (uint)length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] Result(ref Allocator allocator)
        {
            var offset = allocator.offset;
            if (offset == 0)
                return Array.Empty<byte>();
            var result = new byte[offset];
            var buffer = allocator.buffer;
            Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(new Span<byte>(result)), ref MemoryMarshal.GetReference(buffer), (uint)offset);
            return result;
        }
    }
}
