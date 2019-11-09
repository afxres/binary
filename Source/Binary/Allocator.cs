using Mikodev.Binary.Internal;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        private Span<byte> buffer;

        private int offset;

        private int bounds;

        private readonly int limits;

        public readonly int Length => offset;

        public readonly int Capacity => bounds;

        public readonly int MaxCapacity => limits == 0 ? int.MaxValue : ~limits;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Allocator(Span<byte> span) : this(span, int.MaxValue) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Allocator(Span<byte> span, int maxCapacity)
        {
            if (maxCapacity < 0)
                ThrowHelper.ThrowAllocatorMaxCapacityInvalid();
            buffer = span;
            limits = ~maxCapacity;
            offset = 0;
            bounds = Math.Min(span.Length, maxCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsSpan() => buffer.Slice(0, offset);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override string ToString() => $"{nameof(Allocator)}({nameof(Length)}: {Length}, {nameof(Capacity)}: {Capacity}, {nameof(MaxCapacity)}: {MaxCapacity})";
    }
}
