namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Internal.Sequence;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static class SpanLikeNativeEndianMethods
{
    internal static List<E> GetList<E>(ReadOnlySpan<byte> span)
    {
        Debug.Assert(RuntimeHelpers.IsReferenceOrContainsReferences<E>() is false);
        if (span.Length is 0)
            return [];
        var capacity = SequenceContext.GetCapacity<E>(span.Length, Unsafe.SizeOf<E>());
        var result = new List<E>(capacity);
        CollectionsMarshal.SetCount(result, capacity);
        Unsafe.CopyBlockUnaligned(ref Unsafe.As<E, byte>(ref MemoryMarshal.GetReference(CollectionsMarshal.AsSpan(result))), ref MemoryMarshal.GetReference(span), (uint)span.Length);
        return result;
    }

    internal static E[] GetArray<E>(ReadOnlySpan<byte> span)
    {
        Debug.Assert(RuntimeHelpers.IsReferenceOrContainsReferences<E>() is false);
        if (span.Length is 0)
            return [];
        var result = new E[SequenceContext.GetCapacity<E>(span.Length, Unsafe.SizeOf<E>())];
        Unsafe.CopyBlockUnaligned(ref Unsafe.As<E, byte>(ref MemoryMarshal.GetArrayDataReference(result)), ref MemoryMarshal.GetReference(span), (uint)span.Length);
        return result;
    }

    internal static void Encode<E>(ref Allocator allocator, ReadOnlySpan<E> data)
    {
        Debug.Assert(RuntimeHelpers.IsReferenceOrContainsReferences<E>() is false);
        var number = checked(data.Length * Unsafe.SizeOf<E>());
        Allocator.Append(ref allocator, MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<E, byte>(ref MemoryMarshal.GetReference(data)), number));
    }

    internal static void EncodeWithLengthPrefix<E>(ref Allocator allocator, ReadOnlySpan<E> data)
    {
        Debug.Assert(RuntimeHelpers.IsReferenceOrContainsReferences<E>() is false);
        var number = checked(data.Length * Unsafe.SizeOf<E>());
        var numberLength = NumberModule.EncodeLength((uint)number);
        NumberModule.Encode(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
        Allocator.Append(ref allocator, MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<E, byte>(ref MemoryMarshal.GetReference(data)), number));
    }
}
