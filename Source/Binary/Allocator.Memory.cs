using Mikodev.Binary.Internal;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
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

        internal static void AppendLengthPrefix(ref Allocator allocator, int anchor, bool compact)
        {
            const int Limits = 32;
            var offset = allocator.offset;
            // check bounds manually
            if ((ulong)(uint)anchor + sizeof(uint) > (uint)offset)
                ThrowHelper.ThrowAllocatorAnchorInvalid();
            var length = offset - anchor - sizeof(uint);
            var buffer = allocator.buffer;
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), anchor);
            if (compact && length < Limits)
            {
                target = (byte)length;
                allocator.offset = offset - 3;
                BufferHelper.MemoryMoveForward3(ref Unsafe.Add(ref target, sizeof(uint)), (uint)length);
            }
            else
            {
                PrimitiveHelper.EncodeNumberFixed4(ref target, (uint)length);
            }
        }
    }
}
