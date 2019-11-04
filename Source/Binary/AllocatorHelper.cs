using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    public static class AllocatorHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AllocatorAnchor AnchorLengthPrefix(ref Allocator allocator) => new AllocatorAnchor(Allocator.AnchorLengthPrefix(ref allocator));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLengthPrefix(ref Allocator allocator, AllocatorAnchor anchor) => Allocator.AppendLengthPrefix(ref allocator, anchor.Offset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(ref Allocator allocator, ReadOnlySpan<byte> span) => Allocator.AppendBuffer(ref allocator, span);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append<T>(ref Allocator allocator, int length, T data, AllocatorAction<T> action) => Allocator.AppendAction(ref allocator, length, data, action);
    }
}
