using Mikodev.Binary.Internal.Sequence;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal.SpanLike.Adapters
{
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
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref SharedHelper.GetArrayDataReference(result)), ref MemoryMarshal.GetReference(span), (uint)limits);
            return new MemoryBuffer<T>(result, capacity);
        }
    }
}
