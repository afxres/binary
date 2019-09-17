using Mikodev.Binary.Adapters.Abstractions;
using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Adapters.Unsafe
{
    internal sealed class UnsafePrimitiveAdapter<T> : Adapter<T> where T : unmanaged
    {
        public override void OfArray(ref Allocator allocator, in ReadOnlySpan<T> span)
        {
            var itemCount = span.Length;
            if (itemCount == 0)
                return;
            var byteCount = checked(itemCount * Memory.SizeOf<T>());
            ref var target = ref allocator.AllocateReference(byteCount);
            ref var source = ref MemoryMarshal.GetReference(span);
            Endian<T>.Copy(ref target, ref Memory.AsByte(ref source), byteCount);
        }

        public override T[] ToArray(in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return Array.Empty<T>();
            var itemCount = Define.GetItemCount(byteCount, Memory.SizeOf<T>());
            var result = new T[itemCount];
            ref var source = ref MemoryMarshal.GetReference(span);
            ref var target = ref result[0];
            Memory.Copy(ref Memory.AsByte(ref target), ref source, byteCount);
            return result;
        }
    }
}
