namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using Mikodev.Binary.Internal.Sequence;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class NativeEndianAdapter<T> : SpanLikeAdapter<T> where T : unmanaged
{
    public override void Encode(ref Allocator allocator, ReadOnlySpan<T> item)
    {
        Allocator.Append(ref allocator, MemoryMarshal.AsBytes(item));
    }

    public override MemoryBuffer<T> Decode(ReadOnlySpan<byte> span)
    {
        var limits = span.Length;
        if (limits is 0)
            return new MemoryBuffer<T>(Array.Empty<T>(), 0);
        var capacity = SequenceMethods.GetCapacity<T>(limits, Unsafe.SizeOf<T>());
        var result = new T[capacity];
#if NET5_0_OR_GREATER
        Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(result)), ref MemoryMarshal.GetReference(span), (uint)limits);
#else
        Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(new Span<T>(result))), ref MemoryMarshal.GetReference(span), (uint)limits);
#endif
        return new MemoryBuffer<T>(result, capacity);
    }
}
