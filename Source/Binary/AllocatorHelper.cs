using Mikodev.Binary.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public static class AllocatorHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AllocatorAnchor AnchorLengthPrefix(ref Allocator allocator) => new AllocatorAnchor(Allocator.AnchorLengthPrefix(ref allocator));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendLengthPrefix(ref Allocator allocator, AllocatorAnchor anchor) => Allocator.AppendLengthPrefix(ref allocator, anchor.Offset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return;
            ref var target = ref Allocator.AllocateReference(ref allocator, byteCount);
            ref var source = ref MemoryMarshal.GetReference(span);
            MemoryHelper.Copy(ref target, ref source, byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append<T>(ref Allocator allocator, int length, T data, AllocatorAction<T> action)
        {
            if (action == null)
                ThrowHelper.ThrowAllocatorActionInvalid();
            var span = Allocator.Allocate(ref allocator, length);
            action.Invoke(span, data);
        }
    }
}
