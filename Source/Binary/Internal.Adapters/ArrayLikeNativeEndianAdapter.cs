﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class ArrayLikeNativeEndianAdapter<T> : ArrayLikeAdapter<T> where T : unmanaged
    {
        public override void Of(ref Allocator allocator, ReadOnlyMemory<T> memory)
        {
            var origin = memory.Span;
            var source = MemoryMarshal.AsBytes(origin);
            Allocator.AppendBuffer(ref allocator, source);
        }

        public override ArraySegment<T> To(ReadOnlySpan<byte> span)
        {
            var byteLength = span.Length;
            if (byteLength == 0)
                return new ArraySegment<T>(Array.Empty<T>());
            var itemCount = CollectionAdapterHelper.GetItemCount(byteLength, Unsafe.SizeOf<T>(), typeof(T));
            var items = new T[itemCount];
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(new Span<T>(items))), ref MemoryMarshal.GetReference(span), (uint)byteLength);
            return new ArraySegment<T>(items, 0, itemCount);
        }
    }
}
