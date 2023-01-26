namespace Mikodev.Binary.Features.Adapters;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class ConstantListDecoder<E, U> : SpanLikeDecoder<List<E>> where U : struct, IConstantConverterFunctions<E>
{
    public override List<E> Invoke(ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return new List<E>();
        var capacity = SequenceMethods.GetCapacity<E>(span.Length, U.Length);
        var result = new List<E>(capacity);
        ref var source = ref MemoryMarshal.GetReference(span);
        for (var i = 0; i < capacity; i++)
            result.Add(U.Decode(ref Unsafe.Add(ref source, U.Length * i)));
        return result;
    }
}
