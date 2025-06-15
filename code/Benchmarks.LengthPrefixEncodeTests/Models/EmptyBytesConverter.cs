namespace Mikodev.Binary.Benchmarks.LengthPrefixEncodeTests.Models;

using System;

public sealed class EmptyBytesConverter(int length) : Converter<int>(length)
{
    public override int Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

    public override void Encode(ref Allocator allocator, int item) => Allocator.Expand(ref allocator, item);
}
