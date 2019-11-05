using Mikodev.Binary.Internal;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    [DebuggerDisplay(Literals.DebuggerDisplay)]
    public ref partial struct Allocator
    {
        private readonly int limits;

        private int offset;

        private int bounds;

        private byte[] buffer;

        public readonly int Length => offset;

        public readonly int Capacity => bounds;

        public readonly int MaxCapacity => limits == 0 ? int.MaxValue : ~limits;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Allocator(byte[] buffer) : this(buffer, int.MaxValue) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Allocator(byte[] buffer, int maxCapacity)
        {
            if (maxCapacity < 0)
                ThrowHelper.ThrowAllocatorMaxCapacityInvalid();
            this.buffer = buffer;
            offset = 0;
            bounds = Math.Min(buffer == null ? 0 : buffer.Length, maxCapacity);
            limits = ~maxCapacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlyMemory<byte> AsMemory() => new ReadOnlyMemory<byte>(buffer, 0, offset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsSpan() => new ReadOnlySpan<byte>(buffer, 0, offset);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override bool Equals(object obj) => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override int GetHashCode() => throw new NotSupportedException();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly override string ToString() => $"{nameof(Allocator)}({nameof(Length)}: {Length}, {nameof(Capacity)}: {Capacity}, {nameof(MaxCapacity)}: {MaxCapacity})";
    }
}
