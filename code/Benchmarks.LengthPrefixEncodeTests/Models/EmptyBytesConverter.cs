using System;

namespace Mikodev.Binary.Benchmarks.LengthPrefixEncodeTests.Models
{
    public sealed class EmptyBytesConverter : Converter<int>
    {
        public override void Encode(ref Allocator allocator, int item) => AllocatorHelper.Expand(ref allocator, item);

        public override int Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
    }
}
