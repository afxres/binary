namespace Mikodev.Binary.Features.Adapters;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class ConstantDecoder<E, U> : SpanLikeDecoder<E> where U : struct, IConstantConverterFunctions<E>
{
    public override void Decode<T>(SpanLikeDecoderContext<T, E> context, [NotNull] ref T? collection, ReadOnlySpan<byte> span) where T : class
    {
        Debug.Assert(U.Length >= 1);
        Debug.Assert(span.Length is not 0);
        var limits = span.Length;
        var length = U.Length;
        var capacity = SequenceMethods.GetCapacity<E>(limits, length);
        var result = context.Invoke(ref collection, capacity);
        ref var source = ref MemoryMarshal.GetReference(span);
        for (var i = 0; i < capacity; i++)
            result[i] = U.Decode(ref Unsafe.Add(ref source, length * i));
        return;
    }
}
