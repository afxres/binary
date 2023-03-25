namespace Mikodev.Binary.Components;

using System;
using System.Runtime.CompilerServices;

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
}
