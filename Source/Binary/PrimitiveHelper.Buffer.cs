using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        public static void EncodeWithLengthPrefix(ref Allocator allocator, in ReadOnlySpan<byte> span)
        {
            var spanLength = span.Length;
            EncodeNumber(ref allocator, spanLength);
            if (spanLength == 0)
                return;
            ref var target = ref allocator.AllocateReference(spanLength);
            ref var source = ref MemoryMarshal.GetReference(span);
            Memory.Copy(ref target, ref source, spanLength);
        }

        public static ReadOnlySpan<byte> DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            var spanLength = span.Length;
            if (spanLength == 0)
                goto fail;
            ref var location = ref MemoryMarshal.GetReference(span);
            var prefixLength = DecodeNumberLength(location);
            if (spanLength < prefixLength)
                goto fail;
            var length = DecodeNumber(ref location, prefixLength);
            // check bounds via slice method, then replace span with remaining part
            var result = span.Slice(prefixLength, length);
            span = span.Slice(prefixLength + length);
            return result;

        fail:
            ThrowHelper.ThrowLengthPrefixInvalidBytes();
            throw null;
        }
    }
}
