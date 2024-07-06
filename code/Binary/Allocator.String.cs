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
        var limits = encoding.GetMaxByteCount(span.Length);
        ref var target = ref TryCreate(ref allocator, limits);
        if (Unsafe.IsNullRef(ref target))
            target = ref Create(ref allocator, limits = encoding.GetByteCount(span));
        var actual = encoding.GetBytes(span, MemoryMarshal.CreateSpan(ref target, limits));
        if ((uint)actual > (uint)limits)
            ThrowHelper.ThrowInvalidReturnValue();
        Debug.Assert(actual >= 0);
        Debug.Assert(actual <= limits);
        FinishCreate(ref allocator, actual);
    }

    public static void AppendWithLengthPrefix(ref Allocator allocator, scoped ReadOnlySpan<char> span, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(encoding);
        var limits = encoding.GetMaxByteCount(span.Length);
        var numberLength = NumberModule.EncodeLength((uint)limits);
        ref var target = ref TryCreate(ref allocator, limits + numberLength);
        if (Unsafe.IsNullRef(ref target))
        {
            limits = encoding.GetByteCount(span);
            numberLength = NumberModule.EncodeLength((uint)limits);
            target = ref Create(ref allocator, limits + numberLength);
        }
        var actual = encoding.GetBytes(span, MemoryMarshal.CreateSpan(ref Unsafe.Add(ref target, numberLength), limits));
        if ((uint)actual > (uint)limits)
            ThrowHelper.ThrowInvalidReturnValue();
        Debug.Assert(actual >= 0);
        Debug.Assert(actual <= limits);
        NumberModule.Encode(ref target, (uint)actual, numberLength);
        FinishCreate(ref allocator, actual + numberLength);
    }
}
