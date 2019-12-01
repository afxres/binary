using Mikodev.Binary.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        internal static void AppendString(ref Allocator allocator, ReadOnlySpan<char> span, bool withLengthPrefix)
        {
            var encoding = Converter.Encoding;
            var charCount = span.Length;
            ref var chars = ref MemoryMarshal.GetReference(span);
            var maxByteCount = StringHelper.GetMaxByteCountOrByteCount(encoding, ref chars, charCount);
            if (!withLengthPrefix && maxByteCount == 0)
                return;
            var prefixLength = withLengthPrefix ? PrimitiveHelper.EncodeNumberLength((uint)maxByteCount) : 0;
            Ensure(ref allocator, maxByteCount + prefixLength);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            ref var target = ref MemoryMarshal.GetReference(buffer);
            var length = maxByteCount == 0 ? 0 : encoding.GetBytes(ref Unsafe.Add(ref target, offset + prefixLength), maxByteCount, ref chars, charCount);
            if (withLengthPrefix)
                PrimitiveHelper.EncodeNumber(ref Unsafe.Add(ref target, offset), prefixLength, (uint)length);
            allocator.offset = offset + length + prefixLength;
        }
    }
}
