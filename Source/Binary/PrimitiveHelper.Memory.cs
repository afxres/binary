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
            var data = span;
            if (data.IsEmpty)
                return ThrowHelper.ThrowNotEnoughBytesReadOnlySpan<byte>();
            ref var source = ref MemoryMarshal.GetReference(data);
            var numberLength = DecodeNumberLength(source);
            // check bounds via slice method
            data = data.Slice(numberLength);
            var length = DecodeNumber(ref source, numberLength);
            // check bounds via slice method
            span = data.Slice(length);
            return data.Slice(0, length);
        }
    }
}
