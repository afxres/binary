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
            if (targetLimits == 0)
                return;
            Ensure(ref allocator, targetLimits);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
            var targetLength = SharedHelper.GetBytes(span, ref target, targetLimits, encoding);
            allocator.offset = offset + targetLength;
        }

        internal static void AppendStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            var targetLimits = SharedHelper.GetMaxByteCount(span, encoding);
            var prefixLength = PrimitiveHelper.EncodeNumberLength((uint)targetLimits);
            var bufferExpand = targetLimits + prefixLength;
            Ensure(ref allocator, bufferExpand);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
            var targetLength = targetLimits == 0 ? 0 : SharedHelper.GetBytes(span, ref Unsafe.Add(ref target, prefixLength), targetLimits, encoding);
            PrimitiveHelper.EncodeNumber(ref target, prefixLength, (uint)targetLength);
            allocator.offset = offset + targetLength + prefixLength;
        }
    }
}
