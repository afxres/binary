using Mikodev.Binary.Internal;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if !NETOLD
using System.Buffers;
#endif

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        internal static void AppendAnchorAction<T>(ref Allocator allocator, int anchor, int length, T data, SpanAction<byte, T> action)
        {
            if (action is null)
                ThrowHelper.ThrowActionNull();
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            // check bounds via slice method
            var target = buffer.Slice(0, offset).Slice(anchor, length);
            if (length == 0)
                return;
            action.Invoke(target, data);
        }

        internal static void AppendLengthAction<T>(ref Allocator allocator, int length, T data, SpanAction<byte, T> action)
        {
            if (action is null)
                ThrowHelper.ThrowActionNull();
            if (length == 0)
                return;
            var offset = Ensure(ref allocator, length);
            var buffer = allocator.buffer;
            var target = buffer.Slice(offset, length);
            action.Invoke(target, data);
            allocator.offset = offset + length;
        }

        internal static void AppendLengthPrefix(ref Allocator allocator, int anchor)
        {
            const int Limits = 16;
            var offset = allocator.offset;
            // check bounds manually
            if ((ulong)(uint)anchor + sizeof(uint) > (uint)offset)
                ThrowHelper.ThrowAllocatorOrAnchorInvalid();
            var length = offset - anchor - sizeof(uint);
            var buffer = allocator.buffer;
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), anchor);
            if (length <= Limits && buffer.Length - offset >= ((-length) & 7))
            {
                MemoryHelper.EncodeNumber(ref target, (uint)length, numberLength: 1);
                for (var i = 0; i < length; i += 8)
                    MemoryHelper.EncodeNativeEndian(ref Unsafe.Add(ref target, i + 1), MemoryHelper.DecodeNativeEndian<long>(ref Unsafe.Add(ref target, i + 4)));
                allocator.offset = offset - 3;
            }
            else
            {
                MemoryHelper.EncodeNumber(ref target, (uint)length, numberLength: 4);
            }
        }
    }
}
