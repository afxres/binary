namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[DebuggerDisplay(CommonModule.DebuggerDisplayValue)]
public ref partial struct Allocator
{
    private readonly IAllocator? underlying;

    private ref byte target;

    private int bounds;

    private int offset;

    private readonly int limits;

    public readonly int Length => this.offset;

    public readonly int Capacity => this.bounds;

    public readonly int MaxCapacity => this.limits is 0 ? int.MaxValue : ~this.limits;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Allocator(Span<byte> span)
    {
        this.underlying = null;
        this.target = ref MemoryMarshal.GetReference(span);
        this.bounds = span.Length;
        this.offset = 0;
        this.limits = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Allocator(Span<byte> span, int maxCapacity)
    {
        if (maxCapacity < 0)
            ThrowHelper.ThrowMaxCapacityNegative();
        this.underlying = null;
        this.target = ref MemoryMarshal.GetReference(span);
        this.bounds = Math.Min(span.Length, maxCapacity);
        this.offset = 0;
        this.limits = ~maxCapacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Allocator(IAllocator underlyingAllocator)
    {
        ArgumentNullException.ThrowIfNull(underlyingAllocator);
        this.underlying = underlyingAllocator;
        this.target = ref Unsafe.NullRef<byte>();
        this.bounds = 0;
        this.offset = 0;
        this.limits = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Allocator(IAllocator underlyingAllocator, int maxCapacity)
    {
        ArgumentNullException.ThrowIfNull(underlyingAllocator);
        if (maxCapacity < 0)
            ThrowHelper.ThrowMaxCapacityNegative();
        this.underlying = underlyingAllocator;
        this.target = ref Unsafe.NullRef<byte>();
        this.bounds = 0;
        this.offset = 0;
        this.limits = ~maxCapacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly byte[] ToArray()
    {
        var offset = this.offset;
        if (offset is 0)
            return Array.Empty<byte>();
        var result = new byte[offset];
        Unsafe.CopyBlockUnaligned(ref MemoryMarshal.GetArrayDataReference(result), ref this.target, (uint)offset);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<byte> AsSpan() => MemoryMarshal.CreateReadOnlySpan(ref this.target, this.offset);

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(Equals)} on {nameof(Allocator)} will always throw an exception.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    public override readonly bool Equals(object? obj) => throw new NotSupportedException();
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(GetHashCode)} on {nameof(Allocator)} will always throw an exception.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    public override readonly int GetHashCode() => throw new NotSupportedException();
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override readonly string ToString() => $"{nameof(Length)} = {Length}, {nameof(Capacity)} = {Capacity}, {nameof(MaxCapacity)} = {MaxCapacity}";
}
