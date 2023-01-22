namespace Mikodev.Binary.Features.Adapters;

using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Runtime.InteropServices;

internal sealed class NativeEndianEncoder<E> : SpanLikeEncoder<E> where E : unmanaged
{
    public override void Encode(ref Allocator allocator, ReadOnlySpan<E> item)
    {
        Allocator.Append(ref allocator, MemoryMarshal.AsBytes(item));
    }
}
