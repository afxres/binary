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
        int limits;
        if (Enough(ref allocator, (limits = encoding.GetMaxByteCount(span.Length))) is false)
            Ensure(ref allocator, (limits = encoding.GetByteCount(span)));
        ref var target = ref Cursor(ref allocator);
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
        int limits;
        if (Enough(ref allocator, (limits = encoding.GetMaxByteCount(span.Length)) + 4) is false)
            Ensure(ref allocator, (limits = encoding.GetByteCount(span)) + 4);
        if (limits < 0)
            ThrowHelper.ThrowInvalidReturnValue();
        ref var target = ref Cursor(ref allocator);
        var numberLength = NumberModule.EncodeLength((uint)limits);
        var actual = encoding.GetBytes(span, MemoryMarshal.CreateSpan(ref Unsafe.Add(ref target, numberLength), limits));
        if ((uint)actual > (uint)limits)
            ThrowHelper.ThrowInvalidReturnValue();
        Debug.Assert(actual >= 0);
        Debug.Assert(actual <= limits);
        NumberModule.Encode(ref target, (uint)actual, numberLength);
        FinishCreate(ref allocator, actual + numberLength);
    }
}
