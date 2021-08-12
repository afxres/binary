namespace Mikodev.Binary.Benchmarks.LengthPrefixEncodeTests.Models;

using System;

public sealed class EmptyBytesConverter : Converter<int>
{
    public override void Encode(ref Allocator allocator, int item) => Allocator.Expand(ref allocator, item);

    public override int Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
}
