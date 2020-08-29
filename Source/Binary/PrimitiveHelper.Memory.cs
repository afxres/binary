using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        public static void EncodeBufferWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            var length = span.Length;
            var numberLength = MemoryHelper.EncodeNumberLength((uint)length);
            MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)length, numberLength);
            Allocator.AppendBuffer(ref allocator, span);
        }

        public static ReadOnlySpan<byte> DecodeBufferWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            var data = span;
            ref var source = ref MemoryHelper.EnsureLength(data);
            var numberLength = MemoryHelper.DecodeNumberLength(source);
            // check bounds via slice method
            data = data.Slice(numberLength);
            var length = MemoryHelper.DecodeNumber(ref source, numberLength);
            // check bounds via slice method
            span = data.Slice(length);
            return data.Slice(0, length);
        }
    }
}
