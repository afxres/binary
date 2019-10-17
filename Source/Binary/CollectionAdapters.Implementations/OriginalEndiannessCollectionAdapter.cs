using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.CollectionAdapters.Implementations
{
    internal sealed class OriginalEndiannessCollectionAdapter<T> : CollectionAdapter<T> where T : unmanaged
    {
        public override void Of(ref Allocator allocator, in ReadOnlySpan<T> span)
        {
            var spanLength = span.Length;
            var byteLength = checked(spanLength * Memory.SizeOf<T>());
            if (byteLength == 0)
                return;
            ref var target = ref allocator.AllocateReference(byteLength);
            ref var source = ref MemoryMarshal.GetReference(span);
            Memory.Copy(ref target, ref Memory.AsByte(ref source), byteLength);
        }

        public override ArraySegment<T> To(in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return new ArraySegment<T>(Array.Empty<T>());
            var itemCount = CollectionHelper.GetItemCount(byteCount, Memory.SizeOf<T>());
            var items = new T[itemCount];
            ref var source = ref MemoryMarshal.GetReference(span);
            ref var target = ref items[0];
            Memory.Copy(ref Memory.AsByte(ref target), ref source, byteCount);
            return new ArraySegment<T>(items);
        }
    }
}
