using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary
{
    public ref struct Allocator
    {
        #region private fields
        private readonly int bounds;

        private int cursor;

        private int higher;

        private byte[] buffer;
        #endregion

        #region properties
        public int Length => cursor;

        public int Capacity => higher;

        public int MaxCapacity => bounds == 0 ? int.MaxValue : ~bounds;
        #endregion

        #region private methods
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
            higher = target.Length;
            Debug.Assert(higher <= MaxCapacity);
            return target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte[] Ensure(int offset, int expand)
        {
            Debug.Assert(higher >= 0 && higher <= MaxCapacity);
            Debug.Assert(higher == 0 || higher <= buffer.Length);
            return (uint)expand > (uint)(higher - offset) ? Expand(offset, expand) : buffer;
        }
        #endregion

        #region internal methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref byte AllocateReference(int length)
        {
            var offset = cursor;
            var target = Ensure(offset, length);
            cursor = offset + length;
            return ref target[offset];
        }

        internal void AppendWithLengthPrefix(byte[] item)
        {
            Debug.Assert(item != null && item.Length > 0);
            var byteCount = item.Length;
            ref var target = ref AllocateReference(byteCount + sizeof(int));
            ref var source = ref item[0];
            Endian<int>.Set(ref target, byteCount);
            Memory.Copy(ref Memory.Add(ref target, sizeof(int)), ref source, byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LengthPrefixAnchor(out int anchor)
        {
            var offset = cursor;
            var _ = Ensure(offset, sizeof(int));
            var before = offset + sizeof(int);
            cursor = before;
            anchor = before;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LengthPrefixFinish(int anchor)
        {
            Debug.Assert(anchor >= sizeof(int));
            var target = buffer;
            // check array length only (for performance reason, ignore maximum capacity)
            if (target == null || target.Length < anchor)
                ThrowHelper.ThrowAllocatorModified();
            Endian<int>.Set(ref target[anchor - sizeof(int)], cursor - anchor);
        }
        #endregion

        #region constructors
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Allocator(byte[] buffer) : this(buffer, int.MaxValue) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Allocator(byte[] buffer, int maxCapacity)
        {
            if (maxCapacity < 0)
                ThrowHelper.ThrowArgumentOutOfRange(nameof(maxCapacity));
            this.buffer = buffer;
            cursor = 0;
            higher = Math.Min(buffer == null ? 0 : buffer.Length, maxCapacity);
            bounds = ~maxCapacity;
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> Allocate(int length)
        {
            var offset = cursor;
            var target = Ensure(offset, length);
            cursor = offset + length;
            return new Span<byte>(target, offset, length);
        }

        #region as ...
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
        #endregion

        #region append
        public unsafe void Append(in ReadOnlySpan<char> span, Encoding encoding)
        {
            const int Threshold = 32;
            if (encoding == null)
                ThrowHelper.ThrowArgumentNull(nameof(encoding));
            var charCount = span.Length;
            if (charCount == 0)
                return;
            fixed (char* srcptr = &MemoryMarshal.GetReference(span))
            {
                var byteCount = charCount > Threshold
                    ? encoding.GetByteCount(srcptr, charCount)
                    : encoding.GetMaxByteCount(charCount);
                if (byteCount == 0)
                    return;
                var offset = cursor;
                var target = Ensure(offset, byteCount);
                fixed (byte* dstptr = &target[offset])
                    offset += encoding.GetBytes(srcptr, charCount, dstptr, byteCount);
                cursor = offset;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(in ReadOnlySpan<char> span) => Append(span, Converter.Encoding);

        public void Append(in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return;
            ref var target = ref AllocateReference(byteCount);
            ref var source = ref MemoryMarshal.GetReference(span);
            Memory.Copy(ref target, ref source, byteCount);
        }
        #endregion

        #region override
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override bool Equals(object obj) => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override int GetHashCode() => throw new NotSupportedException();

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public override string ToString() => $"{nameof(Allocator)}({nameof(Length)}: {Length}, {nameof(Capacity)}: {Capacity}, {nameof(MaxCapacity)}: {MaxCapacity})";
        #endregion
    }
}
