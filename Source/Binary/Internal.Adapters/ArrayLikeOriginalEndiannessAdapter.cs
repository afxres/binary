using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class ArrayLikeOriginalEndiannessAdapter<T> : ArrayLikeAdapter<T> where T : unmanaged
    {
        public override void Of(ref Allocator allocator, ReadOnlyMemory<T> memory)
        {
            var origin = memory.Span;
            var source = MemoryMarshal.AsBytes(origin);
            Allocator.AppendBuffer(ref allocator, source);
        }

        public override MemoryItem<T> To(ReadOnlySpan<byte> span)
        {
            var byteLength = span.Length;
            if (byteLength == 0)
                return new MemoryItem<T>(Array.Empty<T>(), 0);
            var itemCount = CollectionAdapterHelper.GetItemCount(byteLength, Unsafe.SizeOf<T>(), typeof(T));
            var items = new T[itemCount];
            ref var source = ref MemoryMarshal.GetReference(span);
            ref var target = ref items[0];
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref target), ref source, (uint)byteLength);
            return new MemoryItem<T>(items, itemCount);
        }
    }
}
