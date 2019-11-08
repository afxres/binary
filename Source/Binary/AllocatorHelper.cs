using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    public static class AllocatorHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AllocatorAnchor Anchor(ref Allocator allocator, int length)
        {
            return new AllocatorAnchor(Allocator.AnchorLength(ref allocator, length), length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AllocatorAnchor AnchorLengthPrefix(ref Allocator allocator)
        {
            return new AllocatorAnchor(Allocator.AnchorLengthPrefix(ref allocator), sizeof(int));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append<T>(ref Allocator allocator, AllocatorAnchor anchor, T data, AllocatorAction<T> action)
        {
            Allocator.AppendLength(ref allocator, anchor.Offset, anchor.Length, data, action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLengthPrefix(ref Allocator allocator, AllocatorAnchor anchor)
        {
            Allocator.AppendLengthPrefix(ref allocator, anchor.Offset, anchor.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            Allocator.AppendBuffer(ref allocator, span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append<T>(ref Allocator allocator, int length, T data, AllocatorAction<T> action)
        {
            Allocator.AppendAction(ref allocator, length, data, action);
        }
    }
}
