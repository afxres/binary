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
            return new List<E>();
        var capacity = SequenceMethods.GetCapacity<E>(span.Length, Unsafe.SizeOf<E>());
        var result = new List<E>(capacity);
        ref var source = ref MemoryMarshal.GetReference(span);
        for (var i = 0; i < capacity; i++)
            result.Add(Unsafe.ReadUnaligned<E>(ref Unsafe.Add(ref source, Unsafe.SizeOf<E>() * i)));
        return result;
    }
}
