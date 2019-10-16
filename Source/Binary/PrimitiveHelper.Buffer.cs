using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        public static void EncodeWithLengthPrefix(ref Allocator allocator, in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            EncodeLengthPrefix(ref allocator, (uint)byteCount);
            if (byteCount == 0)
                return;
            ref var target = ref allocator.AllocateReference(byteCount);
            Memory.Copy(ref target, ref MemoryMarshal.GetReference(span), byteCount);
        }

        public static ReadOnlySpan<byte> DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                goto fail;
            ref var location = ref MemoryMarshal.GetReference(span);
            var prefixLength = DecodePrefixLength(location);
            if ((uint)byteCount < (uint)prefixLength)
                goto fail;
            var length = DecodeLengthPrefix(ref location, prefixLength);
            // check bounds via slice method, then replace span with remaining part
            var result = span.Slice(prefixLength, length);
            span = span.Slice(prefixLength + length);
            return result;

        fail:
            _ = ThrowHelper.ThrowLengthPrefixInvalidBytes<int>();
            return default;
        }
    }
}
