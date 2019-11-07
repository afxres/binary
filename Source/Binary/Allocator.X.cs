using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
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
            var length = (long)(source?.Length ?? 0);
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

            var target = new byte[(int)length];
            Debug.Assert(offset <= (source?.Length ?? 0));
            Debug.Assert(offset <= target.Length);
            if (offset != 0)
                Unsafe.CopyBlock(ref target[0], ref source[0], (uint)offset);
            allocator.buffer = target;
            allocator.bounds = target.Length;
            Debug.Assert(allocator.bounds <= limits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Ensure(ref Allocator allocator, int expand)
        {
            Debug.Assert(allocator.bounds >= 0 && allocator.bounds <= allocator.MaxCapacity);
            Debug.Assert(allocator.bounds == 0 || allocator.bounds <= allocator.buffer.Length);
            if (expand <= 0 || (uint)expand > (uint)(allocator.bounds - allocator.offset))
                Expand(ref allocator, expand);
            Debug.Assert(allocator.buffer.Length >= allocator.offset + expand);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref byte Allocate(ref Allocator allocator, int length)
        {
            Ensure(ref allocator, length);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            allocator.offset = offset + length;
            return ref buffer[offset];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int AnchorLengthPrefix(ref Allocator allocator)
        {
            Ensure(ref allocator, sizeof(int));
            var offset = allocator.offset;
            var anchor = offset + sizeof(int);
            allocator.offset = anchor;
            return anchor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AppendLengthPrefix(ref Allocator allocator, int anchor)
        {
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            Debug.Assert(offset >= 0);
            Debug.Assert(offset <= (buffer?.Length ?? 0));
            if (anchor < sizeof(int) || offset < anchor)
                ThrowHelper.ThrowAllocatorOrAnchorInvalid();
            PrimitiveHelper.EncodeNumberFixed4(ref buffer[anchor - sizeof(int)], (uint)(offset - anchor));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AppendBuffer(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return;
            ref var target = ref Allocate(ref allocator, byteCount);
            ref var source = ref MemoryMarshal.GetReference(span);
            Unsafe.CopyBlockUnaligned(ref target, ref source, (uint)byteCount);
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
            var span = new Span<byte>(buffer, offset, length);
            action.Invoke(span, data);
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
            var length = maxByteCount == 0 ? 0 : encoding.GetBytes(ref buffer[offset + prefixLength], maxByteCount, ref chars, charCount);
            if (withLengthPrefix)
                PrimitiveHelper.EncodeNumber(ref buffer[offset], prefixLength, (uint)length);
            allocator.offset = offset + length + prefixLength;
        }
    }
}
