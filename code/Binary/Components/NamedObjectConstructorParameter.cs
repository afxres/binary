namespace Mikodev.Binary.Components;

using Mikodev.Binary.Internal;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[DebuggerDisplay(CommonModule.DebuggerDisplayValue)]
public readonly ref struct NamedObjectConstructorParameter
{
    private readonly ReadOnlySpan<byte> source;

    private readonly Span<long> slices;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal NamedObjectConstructorParameter(ReadOnlySpan<byte> source, Span<long> slices)
    {
        this.source = source;
        this.slices = slices;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool HasValue(int index)
    {
        return this.slices[index] is not 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ReadOnlySpan<byte> GetValue(int index)
    {
        var item = this.slices[index];
        var body = this.source.Slice((int)(item >> 32), (int)item);
        return body;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(Equals)}() on {nameof(NamedObjectConstructorParameter)} will always throw an exception.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    public readonly override bool Equals(object? obj) => throw new NotSupportedException();
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete($"{nameof(GetHashCode)}() on {nameof(NamedObjectConstructorParameter)} will always throw an exception.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    public readonly override int GetHashCode() => throw new NotSupportedException();
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly override string ToString() => $"Value Count = {this.slices.Length}, Memory Length = {this.source.Length}";
}
