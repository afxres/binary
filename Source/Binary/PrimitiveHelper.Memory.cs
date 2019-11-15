using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        public static void EncodeBufferWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            var length = span.Length;
            EncodeNumber(ref allocator, length);
            Allocator.AppendBuffer(ref allocator, span);
        }

        public static ReadOnlySpan<byte> DecodeBufferWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            int numberLength;
            var limits = span.Length;
            ref var source = ref MemoryMarshal.GetReference(span);
            if (limits == 0 || limits < (numberLength = DecodeNumberLength(source)))
                return ThrowHelper.ThrowNotEnoughBytesReadOnlySpan<byte>();
            var length = DecodeNumber(ref source, numberLength);
            // check bounds via slice method
            var result = span.Slice(numberLength, length);
            span = span.Slice(numberLength).Slice(length);
            return result;
        }
    }
}
