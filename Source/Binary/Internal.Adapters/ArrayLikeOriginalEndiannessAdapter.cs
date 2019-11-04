using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class ArrayLikeOriginalEndiannessAdapter<T> : ArrayLikeAdapter<T> where T : unmanaged
    {
        public override void Of(ref Allocator allocator, ReadOnlyMemory<T> memory)
        {
            var span = memory.Span;
            var itemCount = span.Length;
            var byteCount = checked(itemCount * MemoryHelper.SizeOf<T>());
            if (byteCount == 0)
                return;
            ref var target = ref Allocator.Allocate(ref allocator, byteCount);
            ref var source = ref MemoryMarshal.GetReference(span);
            MemoryHelper.Copy(ref target, ref MemoryHelper.AsByte(ref source), byteCount);
        }

        public override MemoryItem<T> To(ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return new MemoryItem<T>(Array.Empty<T>(), 0);
            var itemCount = CollectionAdapterHelper.GetItemCount(byteCount, MemoryHelper.SizeOf<T>());
            var items = new T[itemCount];
            ref var source = ref MemoryMarshal.GetReference(span);
            ref var target = ref items[0];
            MemoryHelper.Copy(ref MemoryHelper.AsByte(ref target), ref source, byteCount);
            return new MemoryItem<T>(items, itemCount);
        }
    }
}
