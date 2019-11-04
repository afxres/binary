using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        public static void EncodeBufferWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            EncodeNumber(ref allocator, byteCount);
            if (byteCount == 0)
                return;
            ref var target = ref Allocator.AllocateReference(ref allocator, byteCount);
            ref var source = ref MemoryMarshal.GetReference(span);
            MemoryHelper.Copy(ref target, ref source, byteCount);
        }

        public static ReadOnlySpan<byte> DecodeBufferWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            var limits = span.Length;
            if (limits == 0)
                goto fail;
            ref var location = ref MemoryMarshal.GetReference(span);
            var prefixLength = DecodeNumberLength(location);
            if (limits < prefixLength)
                goto fail;
            var length = DecodeNumber(ref location, prefixLength);
            // check bounds via slice method, then replace span with remaining part
            var result = span.Slice(prefixLength, length);
            span = span.Slice(prefixLength + length);
            return result;

        fail:
            ThrowHelper.ThrowNotEnoughBytes();
            throw null;
        }
    }
}
