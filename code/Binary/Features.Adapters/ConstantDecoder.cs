namespace Mikodev.Binary.Features.Adapters;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class ConstantDecoder<E, U> : SpanLikeDecoder<E[]> where U : struct, IConstantConverterFunctions<E>
{
    public override E[] Invoke(ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return Array.Empty<E>();
        var result = new E[SequenceContext.GetCapacity<E>(span.Length, U.Length)];
        ref var source = ref MemoryMarshal.GetReference(span);
        for (var i = 0; i < result.Length; i++)
            result[i] = U.Decode(ref Unsafe.Add(ref source, U.Length * i));
        return result;
    }
}
