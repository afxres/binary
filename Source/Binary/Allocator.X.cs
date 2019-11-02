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
        private static byte[] Expand(ref Allocator allocator, int offset, int expand)
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
#if DEBUG
            length = amount;
#else
            const int Origin = 64;
            if (length == 0)
                length = Origin;
            do
                length <<= 2;
            while (length < amount);
#endif
            if (length > limits)
                length = limits;
            Debug.Assert(amount <= length);
            Debug.Assert(length <= limits);

            var target = new byte[(int)length];
            if (offset != 0)
                MemoryHelper.Copy(ref target[0], ref source[0], offset);
            allocator.buffer = target;
            allocator.bounds = target.Length;
            Debug.Assert(allocator.bounds <= limits);
            return target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] Ensure(ref Allocator allocator, int offset, int expand)
        {
            Debug.Assert(allocator.bounds >= 0 && allocator.bounds <= allocator.MaxCapacity);
            Debug.Assert(allocator.bounds == 0 || allocator.bounds <= allocator.buffer.Length);
            return expand <= 0 || (uint)expand > (uint)(allocator.bounds - offset) ? Expand(ref allocator, offset, expand) : allocator.buffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int AnchorLengthPrefix(ref Allocator allocator)
        {
            var offset = allocator.offset;
            _ = Ensure(ref allocator, offset, sizeof(int));
            var before = offset + sizeof(int);
            allocator.offset = before;
            return before;
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
        internal static void Append(ref Allocator allocator, byte[] item)
        {
            Debug.Assert(item != null && item.Length != 0);
            var length = item.Length;
            ref var target = ref AllocateReference(ref allocator, length);
            ref var source = ref item[0];
            MemoryHelper.Copy(ref target, ref source, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<byte> Allocate(ref Allocator allocator, int length)
        {
            var offset = allocator.offset;
            var target = Ensure(ref allocator, offset, length);
            allocator.offset = offset + length;
            return new Span<byte>(target, offset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref byte AllocateReference(ref Allocator allocator, int length)
        {
            var offset = allocator.offset;
            var target = Ensure(ref allocator, offset, length);
            allocator.offset = offset + length;
            return ref target[offset];
        }

        internal static void Encode(ref Allocator allocator, in ReadOnlySpan<char> span, bool withLengthPrefix)
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
