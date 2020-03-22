using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        internal static void AppendString(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            var targetLimits = SharedHelper.GetMaxByteCount(span, encoding);
            Debug.Assert(targetLimits <= encoding.GetMaxByteCount(span.Length));
            if (targetLimits == 0)
                return;
            Ensure(ref allocator, targetLimits);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            var targetLength = SharedHelper.GetBytes(span, buffer.Slice(offset, targetLimits), encoding);
            allocator.offset = offset + targetLength;
        }

        internal static void AppendStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            var targetLimits = SharedHelper.GetMaxByteCount(span, encoding);
            Debug.Assert(targetLimits <= encoding.GetMaxByteCount(span.Length));
            var prefixLength = PrimitiveHelper.EncodeNumberLength((uint)targetLimits);
            Ensure(ref allocator, prefixLength + targetLimits);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            var targetLength = targetLimits == 0 ? 0 : SharedHelper.GetBytes(span, buffer.Slice(offset + prefixLength, targetLimits), encoding);
            PrimitiveHelper.EncodeNumber(ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset), prefixLength, (uint)targetLength);
            allocator.offset = offset + targetLength + prefixLength;
        }
    }
}
