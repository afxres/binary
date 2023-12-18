namespace Mikodev.Binary.Features.Adapters;

using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class NativeEndianListDecoder<E> : SpanLikeDecoder<List<E>> where E : unmanaged
{
    public override List<E> Invoke(ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return [];
        var capacity = SequenceContext.GetCapacity<E>(span.Length, Unsafe.SizeOf<E>());
        var result = new List<E>(capacity);
        CollectionsMarshal.SetCount(result, capacity);
        Unsafe.CopyBlockUnaligned(ref Unsafe.As<E, byte>(ref MemoryMarshal.GetReference(CollectionsMarshal.AsSpan(result))), ref MemoryMarshal.GetReference(span), (uint)span.Length);
        return result;
    }
}
