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
        private static void Expand(ref Allocator allocator, int offset, int expand)
        {
            if (expand <= 0)
                ThrowHelper.ThrowArgumentLengthInvalid();
            Debug.Assert(offset >= 0);
            var limits = allocator.MaxCapacity;
            var amount = ((long)(uint)offset) + ((uint)expand);
            if (amount > limits)
                ThrowHelper.ThrowAllocatorOverflow();

            var source = allocator.buffer;
            var length = (long)(source == null ? 0 : source.Length);
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
        private static byte[] Ensure(ref Allocator allocator, int offset, int expand)
        {
            Debug.Assert(allocator.bounds >= 0 && allocator.bounds <= allocator.MaxCapacity);
            Debug.Assert(allocator.bounds == 0 || allocator.bounds <= allocator.buffer.Length);
            if (expand <= 0 || (uint)expand > (uint)(allocator.bounds - offset))
                Expand(ref allocator, offset, expand);
            Debug.Assert(allocator.buffer.Length >= offset + expand);
            return allocator.buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref byte Allocate(ref Allocator allocator, int length)
        {
            var offset = allocator.offset;
            var target = Ensure(ref allocator, offset, length);
            allocator.offset = offset + length;
            return ref target[offset];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int AnchorLengthPrefix(ref Allocator allocator)
        {
            var offset = allocator.offset;
            _ = Ensure(ref allocator, offset, sizeof(int));
            var anchor = offset + sizeof(int);
            allocator.offset = anchor;
            return anchor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AppendLengthPrefix(ref Allocator allocator, int anchor)
        {
            var target = allocator.buffer;
            var offset = allocator.offset;
            Debug.Assert(offset >= 0);
            Debug.Assert(offset <= (target?.Length ?? 0));
            if (anchor < sizeof(int) || offset < anchor)
                ThrowHelper.ThrowAllocatorOrAnchorInvalid();
            PrimitiveHelper.EncodeNumberFixed4(ref target[anchor - sizeof(int)], (uint)(offset - anchor));
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
            var offset = allocator.offset;
            var target = Ensure(ref allocator, offset, length);
            var span = new Span<byte>(target, offset, length);
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
            var offset = allocator.offset;
            var target = Ensure(ref allocator, offset, maxByteCount + prefixLength);
            var length = maxByteCount == 0 ? 0 : encoding.GetBytes(ref target[offset + prefixLength], maxByteCount, ref chars, charCount);
            if (withLengthPrefix)
                PrimitiveHelper.EncodeNumber(ref target[offset], prefixLength, (uint)length);
            allocator.offset = offset + length + prefixLength;
        }
    }
}
