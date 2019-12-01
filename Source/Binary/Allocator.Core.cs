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
            Ensure(ref allocator, sizeof(byte));
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset) = item;
            allocator.offset = offset + sizeof(byte);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AppendBuffer(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            var length = span.Length;
            if (length == 0)
                return;
            Ensure(ref allocator, length);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
            ref var source = ref MemoryMarshal.GetReference(span);
            Unsafe.CopyBlockUnaligned(ref target, ref source, (uint)length);
            allocator.offset = offset + length;
        }

        internal static void AppendLengthPrefix(ref Allocator allocator, int anchor, bool compact)
        {
            const int Limits = 33;
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            // check bounds manually
            if ((ulong)(uint)anchor + sizeof(uint) > (uint)offset)
                ThrowHelper.ThrowAllocatorAnchorInvalid();
            ref var location = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), anchor);
            var length = offset - anchor - sizeof(uint);
            if (compact && length < Limits)
            {
                if (length > 0)
                    Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref location, sizeof(byte)), ref Unsafe.Add(ref location, sizeof(uint)), (uint)length);
                location = (byte)length;
                allocator.offset = offset - (sizeof(uint) - sizeof(byte));
            }
            else
            {
                PrimitiveHelper.EncodeNumberFixed4(ref location, (uint)length);
            }
        }
    }
}
