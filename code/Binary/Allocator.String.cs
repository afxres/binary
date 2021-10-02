namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public ref partial struct Allocator
{
    public static void Append(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
    {
        if (encoding is null)
            ThrowHelper.ThrowEncodingNull();
        var targetLimits = SharedModule.GetMaxByteCount(span, encoding);
        Debug.Assert(targetLimits <= encoding.GetMaxByteCount(span.Length));
        if (targetLimits is 0)
            return;
        Ensure(ref allocator, targetLimits);
        var offset = allocator.offset;
        var buffer = allocator.buffer;
        ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
        var targetLength = encoding.GetBytes(span, MemoryMarshal.CreateSpan(ref target, targetLimits));
        allocator.offset = offset + targetLength;
    }

    public static void AppendWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
    {
        if (encoding is null)
            ThrowHelper.ThrowEncodingNull();
        var targetLimits = SharedModule.GetMaxByteCount(span, encoding);
        Debug.Assert(targetLimits <= encoding.GetMaxByteCount(span.Length));
        var prefixLength = NumberHelper.EncodeLength((uint)targetLimits);
        Ensure(ref allocator, prefixLength + targetLimits);
        var offset = allocator.offset;
        var buffer = allocator.buffer;
        ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
        var targetLength = targetLimits is 0 ? 0 : encoding.GetBytes(span, MemoryMarshal.CreateSpan(ref Unsafe.Add(ref target, prefixLength), targetLimits));
        NumberHelper.Encode(ref target, (uint)targetLength, prefixLength);
        allocator.offset = offset + targetLength + prefixLength;
    }
}
