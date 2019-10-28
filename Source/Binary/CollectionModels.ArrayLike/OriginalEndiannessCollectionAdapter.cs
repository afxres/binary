using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.CollectionModels.ArrayLike
{
    internal sealed class OriginalEndiannessCollectionAdapter<T> : ArrayLikeAdapter<T> where T : unmanaged
    {
        public override void Of(ref Allocator allocator, ReadOnlyMemory<T> memory)
        {
            var span = memory.Span;
            var itemCount = span.Length;
            var byteCount = checked(itemCount * Memory.SizeOf<T>());
            if (byteCount == 0)
                return;
            ref var target = ref allocator.AllocateReference(byteCount);
            ref var source = ref MemoryMarshal.GetReference(span);
            Memory.Copy(ref target, ref Memory.AsByte(ref source), byteCount);
        }

        public override MemoryItem<T> To(in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return new MemoryItem<T>(Array.Empty<T>(), 0);
            var itemCount = CollectionHelper.GetItemCount(byteCount, Memory.SizeOf<T>());
            var items = new T[itemCount];
            ref var source = ref MemoryMarshal.GetReference(span);
            ref var target = ref items[0];
            Memory.Copy(ref Memory.AsByte(ref target), ref source, byteCount);
            return new MemoryItem<T>(items, itemCount);
        }
    }
}
