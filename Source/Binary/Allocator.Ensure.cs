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
            var amount = ((long)(uint)offset) + ((uint)expand);
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
        private static void Ensure(ref Allocator allocator, int expand)
        {
            if ((ulong)(uint)allocator.offset + (uint)expand > (uint)allocator.buffer.Length)
                Expand(ref allocator, expand);
            Debug.Assert(allocator.buffer.Length <= allocator.MaxCapacity);
            Debug.Assert(allocator.buffer.Length >= allocator.offset + expand);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref byte Assign(ref Allocator allocator, int length)
        {
            Debug.Assert(length > 0);
            Ensure(ref allocator, length);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            allocator.offset = offset + length;
            return ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Anchor(ref Allocator allocator, int length)
        {
            var offset = allocator.offset;
            if (length == 0)
                return offset;
            Ensure(ref allocator, length);
            allocator.offset = offset + length;
            return offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Append(ref Allocator allocator, byte item)
        {
            // write unaligned
            ref var target = ref Assign(ref allocator, sizeof(byte));
            Unsafe.WriteUnaligned(ref target, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AppendBuffer(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            var length = span.Length;
            if (length == 0)
                return;
            ref var target = ref Assign(ref allocator, length);
            ref var source = ref MemoryMarshal.GetReference(span);
            Unsafe.CopyBlockUnaligned(ref target, ref source, (uint)length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] Detach(ref Allocator allocator)
        {
            var offset = allocator.offset;
            if (offset == 0)
                return Array.Empty<byte>();
            var result = new byte[offset];
            var buffer = allocator.buffer;
            Unsafe.CopyBlockUnaligned(ref result[0], ref MemoryMarshal.GetReference(buffer), (uint)offset);
            return result;
        }
    }
}
