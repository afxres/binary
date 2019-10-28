using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Extensions;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        private readonly int limits;

        private int cursor;

        private int bounds;

        private byte[] buffer;

        public int Length => cursor;

        public int Capacity => bounds;

        public int MaxCapacity => limits == 0 ? int.MaxValue : ~limits;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private byte[] Expand(int offset, int expand)
        {
            Debug.Assert(offset >= 0);
            var limits = MaxCapacity;
            var amount = ((long)(uint)offset) + ((uint)expand);
            if (amount > limits)
                ThrowHelper.ThrowAllocatorOverflow();

            var source = buffer;
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
                Memory.Copy(target, source, offset);
            buffer = target;
            bounds = target.Length;
            Debug.Assert(bounds <= MaxCapacity);
            return target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] Ensure(int offset, int expand)
        {
            Debug.Assert(bounds >= 0 && bounds <= MaxCapacity);
            Debug.Assert(bounds == 0 || bounds <= buffer.Length);
            return (uint)expand > (uint)(bounds - offset) ? Expand(offset, expand) : buffer;
        }

        internal void Encode(in ReadOnlySpan<char> span, bool withLengthPrefix)
        {
            var encoding = Converter.Encoding;
            var charCount = span.Length;
            ref var chars = ref MemoryMarshal.GetReference(span);
            var maxByteCount = StringHelper.GetMaxByteCountOrByteCount(encoding, ref chars, charCount);
            if (!withLengthPrefix && maxByteCount == 0)
                return;
            var prefixLength = withLengthPrefix ? PrimitiveHelper.EncodeNumberLength((uint)maxByteCount) : 0;
            var offset = cursor;
            var target = Ensure(offset, maxByteCount + prefixLength);
            var length = maxByteCount == 0 ? 0 : encoding.GetBytes(ref target[offset + prefixLength], maxByteCount, ref chars, charCount);
            if (withLengthPrefix)
                PrimitiveHelper.EncodeNumber(ref target[offset], prefixLength, (uint)length);
            cursor = offset + length + prefixLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Append(byte[] item)
        {
            Debug.Assert(item != null && item.Length != 0);
            var length = item.Length;
            ref var target = ref AllocateReference(length);
            ref var source = ref item[0];
            Memory.Copy(ref target, ref source, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int AnchorLengthPrefix()
        {
            var offset = cursor;
            _ = Ensure(offset, sizeof(int));
            var before = offset + sizeof(int);
            cursor = before;
            return before;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AppendLengthPrefix(int anchor)
        {
            var target = buffer;
            var offset = cursor;
            var length = offset - anchor;
            var origin = anchor - sizeof(int);
            // check bounds (for performance reason, ignore maximum capacity)
            if (length < 0 || origin < 0 || target == null || target.Length < anchor)
                ThrowHelper.ThrowAllocatorModified();
            PrimitiveHelper.EncodeNumberFixed4(ref target[origin], (uint)length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Span<byte> Allocate(int length)
        {
            var offset = cursor;
            var target = Ensure(offset, length);
            cursor = offset + length;
            return new Span<byte>(target, offset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref byte AllocateReference(int length)
        {
            var offset = cursor;
            var target = Ensure(offset, length);
            cursor = offset + length;
            return ref target[offset];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Allocator(byte[] buffer) : this(buffer, int.MaxValue) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Allocator(byte[] buffer, int maxCapacity)
        {
            if (maxCapacity < 0)
                ThrowHelper.ThrowAllocatorMaxCapacityInvalid();
            this.buffer = buffer;
            cursor = 0;
            bounds = Math.Min(buffer == null ? 0 : buffer.Length, maxCapacity);
            limits = ~maxCapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<byte> AsMemory() => new ReadOnlyMemory<byte>(buffer, 0, cursor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> AsSpan() => new ReadOnlySpan<byte>(buffer, 0, cursor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] ToArray()
        {
            var offset = cursor;
            if (offset == 0)
                return Array.Empty<byte>();
            var source = buffer;
            var target = new byte[offset];
            Memory.Copy(target, source, offset);
            return target;
        }

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override bool Equals(object obj) => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override int GetHashCode() => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override string ToString() => $"{nameof(Allocator)}({nameof(Length)}: {Length}, {nameof(Capacity)}: {Capacity}, {nameof(MaxCapacity)}: {MaxCapacity})";
    }
}
