using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary
{
    public static class AllocatorHelper
    {
        public static AllocatorAnchor Anchor(ref Allocator allocator, int length)
        {
            return new AllocatorAnchor(Allocator.Anchor(ref allocator, length), length);
        }

        public static AllocatorAnchor AnchorLengthPrefix(ref Allocator allocator)
        {
            return new AllocatorAnchor(Allocator.Anchor(ref allocator, sizeof(int)), sizeof(int));
        }

        public static void Append<T>(ref Allocator allocator, AllocatorAnchor anchor, T data, AllocatorAction<T> action)
        {
            Allocator.AppendLength(ref allocator, anchor.Offset, anchor.Length, data, action);
        }

        public static void AppendLengthPrefix(ref Allocator allocator, AllocatorAnchor anchor)
        {
            if (anchor.Length != sizeof(int))
                ThrowHelper.ThrowAllocatorAnchorInvalid();
            Allocator.AppendLengthPrefix(ref allocator, anchor.Offset, compact: false);
        }

        public static void Append(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            Allocator.AppendBuffer(ref allocator, span);
        }

        public static void Append<T>(ref Allocator allocator, int length, T data, AllocatorAction<T> action)
        {
            Allocator.AppendAction(ref allocator, length, data, action);
        }
    }
}
