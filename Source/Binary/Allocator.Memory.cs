using Mikodev.Binary.Internal;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AppendLength<T>(ref Allocator allocator, int anchor, int length, T data, AllocatorAction<T> action)
        {
            if (action is null)
                ThrowHelper.ThrowAllocatorActionInvalid();
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            // check bounds via slice method
            var target = buffer.Slice(0, offset).Slice(anchor, length);
            if (length == 0)
                return;
            action.Invoke(target, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AppendAction<T>(ref Allocator allocator, int length, T data, AllocatorAction<T> action)
        {
            if (action is null)
                ThrowHelper.ThrowAllocatorActionInvalid();
            if (length == 0)
                return;
            Ensure(ref allocator, length);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            var target = buffer.Slice(offset, length);
            action.Invoke(target, data);
            allocator.offset = offset + length;
        }
    }
}
