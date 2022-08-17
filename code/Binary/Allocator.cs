namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public ref partial struct Allocator
{
#if NET7_0_OR_GREATER
    private ref byte values;

    private int bounds;
#else
    private Span<byte> buffer;
#endif

    private int offset;

    private readonly int limits;

    public readonly int Length => this.offset;

#if NET7_0_OR_GREATER
    public readonly int Capacity => this.bounds;
#else
    public readonly int Capacity => this.buffer.Length;
#endif

    public readonly int MaxCapacity => this.limits is 0 ? int.MaxValue : ~this.limits;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Allocator(Span<byte> span)
    {
        this.limits = 0;
        this.offset = 0;
#if NET7_0_OR_GREATER
        this.bounds = span.Length;
        this.values = ref MemoryMarshal.GetReference(span);
#else
        this.buffer = span;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Allocator(Span<byte> span, int maxCapacity)
    {
        if (maxCapacity < 0)
            ThrowHelper.ThrowMaxCapacityNegative();
        this.limits = ~maxCapacity;
        this.offset = 0;
#if NET7_0_OR_GREATER
        this.bounds = Math.Min(span.Length, maxCapacity);
        this.values = ref MemoryMarshal.GetReference(span);
#else
        this.buffer = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(span), Math.Min(span.Length, maxCapacity));
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly byte[] ToArray()
    {
        var offset = this.offset;
        if (offset is 0)
            return Array.Empty<byte>();
        var result = new byte[offset];
#if NET7_0_OR_GREATER
        Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetArrayDataReference(result), ref this.values, (uint)offset);
#else
        var buffer = this.buffer;
        Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetArrayDataReference(result), ref MemoryMarshal.GetReference(buffer), (uint)offset);
#endif
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NET7_0_OR_GREATER
    public readonly ReadOnlySpan<byte> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref this.values, this.offset);
#else
    public readonly ReadOnlySpan<byte> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(this.buffer), this.offset);
#endif

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(Equals)} on {nameof(Allocator)} will always throw an exception.")]
    public override readonly bool Equals(object? obj) => throw new NotSupportedException();

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(GetHashCode)} on {nameof(Allocator)} will always throw an exception.")]
    public override readonly int GetHashCode() => throw new NotSupportedException();

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly string ToString() => $"{nameof(Allocator)}({nameof(Length)}: {Length}, {nameof(Capacity)}: {Capacity}, {nameof(MaxCapacity)}: {MaxCapacity})";
}
