using Mikodev.Binary.Internal.Sequence;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal.SpanLike.Adapters
{
    internal sealed class SpanLikeNativeEndianAdapter<T> : SpanLikeAdapter<T> where T : unmanaged
    {
        public override void Encode(ref Allocator allocator, ReadOnlySpan<T> item)
        {
            Allocator.Append(ref allocator, MemoryMarshal.AsBytes(item));
        }

        public override MemoryResult<T> Decode(ReadOnlySpan<byte> span)
        {
            var byteLength = span.Length;
            if (byteLength is 0)
                return new MemoryResult<T>(Array.Empty<T>(), 0);
            var itemLength = Unsafe.SizeOf<T>();
            var capacity = SequenceMethods.GetCapacity<T>(byteLength, itemLength);
            var collection = new T[capacity];
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(new Span<T>(collection))), ref MemoryMarshal.GetReference(span), (uint)byteLength);
            return new MemoryResult<T>(collection, capacity);
        }
    }
}
