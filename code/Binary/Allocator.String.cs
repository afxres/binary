using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        internal static void AppendString(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            var targetLimits = SharedHelper.GetMaxByteCount(span);
            Debug.Assert(targetLimits <= SharedHelper.Encoding.GetMaxByteCount(span.Length));
            if (targetLimits is 0)
                return;
            Ensure(ref allocator, targetLimits);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
            var targetLength = SharedHelper.Encoding.GetBytes(span, MemoryMarshal.CreateSpan(ref target, targetLimits));
            allocator.offset = offset + targetLength;
        }

        internal static void AppendStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            var targetLimits = SharedHelper.GetMaxByteCount(span);
            Debug.Assert(targetLimits <= SharedHelper.Encoding.GetMaxByteCount(span.Length));
            var prefixLength = MemoryHelper.EncodeNumberLength((uint)targetLimits);
            Ensure(ref allocator, prefixLength + targetLimits);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
            var targetLength = targetLimits is 0 ? 0 : SharedHelper.Encoding.GetBytes(span, MemoryMarshal.CreateSpan(ref Unsafe.Add(ref target, prefixLength), targetLimits));
            MemoryHelper.EncodeNumber(ref target, (uint)targetLength, prefixLength);
            allocator.offset = offset + targetLength + prefixLength;
        }
    }
}
