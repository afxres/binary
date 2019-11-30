﻿using Mikodev.Binary.Internal;
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
        internal static ref byte Allocate(ref Allocator allocator, int length)
        {
            Debug.Assert(length > 0);
            Ensure(ref allocator, length);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            allocator.offset = offset + length;
            return ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
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

        internal static void AppendLength<T>(ref Allocator allocator, int anchor, int length, T data, AllocatorAction<T> action)
        {
            if (action == null)
                ThrowHelper.ThrowAllocatorActionInvalid();
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            // check bounds via slice method
            var target = buffer.Slice(0, offset).Slice(anchor, length);
            if (length == 0)
                return;
            action.Invoke(target, data);
        }

        internal static void AppendAction<T>(ref Allocator allocator, int length, T data, AllocatorAction<T> action)
        {
            if (action == null)
                ThrowHelper.ThrowAllocatorActionInvalid();
            if (length == 0)
                return;
            Ensure(ref allocator, length);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            var target = buffer.Slice(offset, length);
            action.Invoke(target, data);
            allocator.offset = offset + length;
        }

        internal static void AppendString(ref Allocator allocator, ReadOnlySpan<char> span, bool withLengthPrefix)
        {
            var encoding = Converter.Encoding;
            var charCount = span.Length;
            ref var chars = ref MemoryMarshal.GetReference(span);
            var maxByteCount = StringHelper.GetMaxByteCountOrByteCount(encoding, ref chars, charCount);
            if (!withLengthPrefix && maxByteCount == 0)
                return;
            var prefixLength = withLengthPrefix ? PrimitiveHelper.EncodeNumberLength((uint)maxByteCount) : 0;
            Ensure(ref allocator, maxByteCount + prefixLength);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            ref var target = ref MemoryMarshal.GetReference(buffer);
            var length = maxByteCount == 0 ? 0 : encoding.GetBytes(ref Unsafe.Add(ref target, offset + prefixLength), maxByteCount, ref chars, charCount);
            if (withLengthPrefix)
                PrimitiveHelper.EncodeNumber(ref Unsafe.Add(ref target, offset), prefixLength, (uint)length);
            allocator.offset = offset + length + prefixLength;
        }
    }
}
