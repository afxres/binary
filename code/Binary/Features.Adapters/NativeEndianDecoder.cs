namespace Mikodev.Binary.Features.Adapters;

using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class NativeEndianDecoder<E> : SpanLikeDecoder<E[]> where E : unmanaged
{
    public override E[] Invoke(ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return [];
        var result = new E[SequenceContext.GetCapacity<E>(span.Length, Unsafe.SizeOf<E>())];
        Unsafe.CopyBlockUnaligned(ref Unsafe.As<E, byte>(ref MemoryMarshal.GetArrayDataReference(result)), ref MemoryMarshal.GetReference(span), (uint)span.Length);
        return result;
    }
}
