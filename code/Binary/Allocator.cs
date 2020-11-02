using Mikodev.Binary.Internal;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        private Span<byte> buffer;

        private int offset;

        private readonly int limits;

        public readonly int Length => this.offset;

        public readonly int Capacity => this.buffer.Length;

        public readonly int MaxCapacity => this.limits is 0 ? int.MaxValue : ~this.limits;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Allocator(Span<byte> span)
        {
            this.limits = 0;
            this.offset = 0;
            this.buffer = span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Allocator(Span<byte> span, int maxCapacity)
        {
            if (maxCapacity < 0)
                ThrowHelper.ThrowMaxCapacityNegative();
            this.limits = ~maxCapacity;
            this.offset = 0;
            this.buffer = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(span), Math.Min(span.Length, maxCapacity));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly byte[] ToArray()
        {
            var offset = this.offset;
            if (offset is 0)
                return Array.Empty<byte>();
            var buffer = this.buffer;
            var result = new byte[offset];
            Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetReference(new Span<byte>(result)), ref MemoryMarshal.GetReference(buffer), (uint)offset);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(this.buffer), this.offset);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly string ToString() => $"{nameof(Allocator)}({nameof(Length)}: {Length}, {nameof(Capacity)}: {Capacity}, {nameof(MaxCapacity)}: {MaxCapacity})";
    }
}
