namespace Mikodev.Binary.Internal.Metadata;

using System;

internal sealed class IConverterPlaceholder : IConverter
{
    public int Length => throw new NotSupportedException();

    public void Encode(ref Allocator allocator, object? item) => throw new NotSupportedException();

    public void EncodeAuto(ref Allocator allocator, object? item) => throw new NotSupportedException();

    public void EncodeWithLengthPrefix(ref Allocator allocator, object? item) => throw new NotSupportedException();

    public byte[] Encode(object? item) => throw new NotSupportedException();

    public object? Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

    public object? DecodeAuto(ref ReadOnlySpan<byte> span) => throw new NotSupportedException();

    public object? DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => throw new NotSupportedException();

    public object? Decode(byte[]? buffer) => throw new NotSupportedException();

    public static IConverterPlaceholder Instance { get; } = new IConverterPlaceholder();
}
