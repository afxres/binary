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
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
            var targetLength = SharedHelper.GetBytes(ref MemoryMarshal.GetReference(span), span.Length, ref target, targetLimits, encoding);
            allocator.offset = offset + targetLength;
        }

        internal static void AppendStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            var targetLimits = SharedHelper.GetMaxByteCount(span, encoding);
            Debug.Assert(targetLimits <= encoding.GetMaxByteCount(span.Length));
            var prefixLength = MemoryHelper.EncodeNumberLength((uint)targetLimits);
            Ensure(ref allocator, prefixLength + targetLimits);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
            var targetLength = targetLimits == 0 ? 0 : SharedHelper.GetBytes(ref MemoryMarshal.GetReference(span), span.Length, ref Unsafe.Add(ref target, prefixLength), targetLimits, encoding);
            MemoryHelper.EncodeNumber(ref target, (uint)targetLength, prefixLength);
            allocator.offset = offset + targetLength + prefixLength;
        }
    }
}
