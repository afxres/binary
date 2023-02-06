namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

public ref partial struct Allocator
{
    public static void Append(ref Allocator allocator, scoped ReadOnlySpan<char> span, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);
        var targetLimits = SharedModule.GetMaxByteCount(span, encoding);
        Debug.Assert(targetLimits <= encoding.GetMaxByteCount(span.Length));
        if (targetLimits is 0)
            return;
        ref var target = ref Create(ref allocator, targetLimits);
        var targetLength = encoding.GetBytes(span, MemoryMarshal.CreateSpan(ref target, targetLimits));
        FinishCreate(ref allocator, targetLength);
    }

    public static void AppendWithLengthPrefix(ref Allocator allocator, scoped ReadOnlySpan<char> span, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);
        var targetLimits = SharedModule.GetMaxByteCount(span, encoding);
        Debug.Assert(targetLimits <= encoding.GetMaxByteCount(span.Length));
        var prefixLength = NumberModule.EncodeLength((uint)targetLimits);
        ref var target = ref Create(ref allocator, prefixLength + targetLimits);
        var targetLength = targetLimits is 0 ? 0 : encoding.GetBytes(span, MemoryMarshal.CreateSpan(ref Unsafe.Add(ref target, prefixLength), targetLimits));
        NumberModule.Encode(ref target, (uint)targetLength, prefixLength);
        FinishCreate(ref allocator, targetLength + prefixLength);
    }
}
