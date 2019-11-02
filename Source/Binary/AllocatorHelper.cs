using Mikodev.Binary.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public static class AllocatorHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Allocate(ref Allocator allocator, int length) => Allocator.Allocate(ref allocator, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref byte AllocateReference(ref Allocator allocator, int length) => ref Allocator.AllocateReference(ref allocator, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(ref Allocator allocator, in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return;
            ref var target = ref AllocateReference(ref allocator, byteCount);
            ref var source = ref MemoryMarshal.GetReference(span);
            MemoryHelper.Copy(ref target, ref source, byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AllocatorAnchor AnchorLengthPrefix(ref Allocator allocator) => new AllocatorAnchor(Allocator.AnchorLengthPrefix(ref allocator));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLengthPrefix(ref Allocator allocator, AllocatorAnchor anchor) => Allocator.AppendLengthPrefix(ref allocator, anchor.Offset);
    }
}
