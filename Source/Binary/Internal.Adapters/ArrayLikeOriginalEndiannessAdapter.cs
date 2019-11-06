using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class ArrayLikeOriginalEndiannessAdapter<T> : ArrayLikeAdapter<T> where T : unmanaged
    {
        public override void Of(ref Allocator allocator, ReadOnlyMemory<T> memory)
        {
            var span = memory.Span;
            var itemCount = span.Length;
            var byteCount = checked(itemCount * Unsafe.SizeOf<T>());
            if (byteCount == 0)
                return;
            ref var target = ref Allocator.Allocate(ref allocator, byteCount);
            ref var source = ref MemoryMarshal.GetReference(span);
            Unsafe.CopyBlockUnaligned(ref target, ref Unsafe.As<T, byte>(ref source), (uint)byteCount);
        }

        public override MemoryItem<T> To(ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return new MemoryItem<T>(Array.Empty<T>(), 0);
            var itemCount = CollectionAdapterHelper.GetItemCount(byteCount, Unsafe.SizeOf<T>());
            var items = new T[itemCount];
            ref var source = ref MemoryMarshal.GetReference(span);
            ref var target = ref items[0];
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref target), ref source, (uint)byteCount);
            return new MemoryItem<T>(items, itemCount);
        }
    }
}
