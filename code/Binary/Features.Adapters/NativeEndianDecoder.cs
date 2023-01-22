namespace Mikodev.Binary.Features.Adapters;

using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class NativeEndianDecoder<E> : SpanLikeDecoder<E> where E : unmanaged
{
    public override void Decode<T>(SpanLikeDecoderContext<T, E> context, [NotNull] ref T? collection, ReadOnlySpan<byte> span) where T : class
    {
        Debug.Assert(span.Length is not 0);
        var limits = span.Length;
        var capacity = SequenceMethods.GetCapacity<E>(limits, Unsafe.SizeOf<E>());
        var result = context.Invoke(ref collection, capacity);
        Unsafe.CopyBlockUnaligned(ref Unsafe.As<E, byte>(ref MemoryMarshal.GetReference(result)), ref MemoryMarshal.GetReference(span), (uint)limits);
    }
}
