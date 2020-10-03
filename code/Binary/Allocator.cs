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

        public readonly int Length => offset;

        public readonly int Capacity => buffer.Length;

        public readonly int MaxCapacity => limits == 0 ? int.MaxValue : ~limits;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Allocator(Span<byte> span)
        {
            limits = 0;
            offset = 0;
            buffer = span;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Allocator(Span<byte> span, int maxCapacity)
        {
            if (maxCapacity < 0)
                ThrowHelper.ThrowMaxCapacityNegative();
            limits = ~maxCapacity;
            offset = 0;
            buffer = span.Slice(0, Math.Min(span.Length, maxCapacity));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsSpan() => buffer.Slice(0, offset);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly ref readonly byte GetPinnableReference() => ref MemoryMarshal.GetReference(buffer);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override readonly string ToString() => $"{nameof(Allocator)}({nameof(Length)}: {Length}, {nameof(Capacity)}: {Capacity}, {nameof(MaxCapacity)}: {MaxCapacity})";
    }
}
