using Mikodev.Binary.Internal;
using System;

#if !NETOLD
using System.Buffers;
#endif

namespace Mikodev.Binary
{
    public static class AllocatorHelper
    {
        public static AllocatorAnchor Anchor(ref Allocator allocator, int length)
        {
            return new AllocatorAnchor(Allocator.Anchor(ref allocator, length), length);
        }

        public static void Append(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            Allocator.AppendBuffer(ref allocator, span);
        }

        public static void Append<T>(ref Allocator allocator, AllocatorAnchor anchor, T data, SpanAction<byte, T> action)
        {
            Allocator.AppendAnchorAction(ref allocator, anchor.Offset, anchor.Length, data, action);
        }

        public static void Append<T>(ref Allocator allocator, int length, T data, SpanAction<byte, T> action)
        {
            Allocator.AppendLengthAction(ref allocator, length, data, action);
        }

        public static void AppendWithLengthPrefix<T>(ref Allocator allocator, T data, AllocatorAction<T> action)
        {
            if (action is null)
                ThrowHelper.ThrowActionNull();
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            action.Invoke(ref allocator, data);
            Allocator.AppendLengthPrefix(ref allocator, anchor);
        }

        public static byte[] Invoke<T>(T data, AllocatorAction<T> action)
        {
            if (action is null)
                ThrowHelper.ThrowActionNull();
            var memory = BufferHelper.Borrow();
            try
            {
                var allocator = new Allocator(BufferHelper.Intent(memory));
                action.Invoke(ref allocator, data);
                return Allocator.Result(ref allocator);
            }
            finally
            {
                BufferHelper.Return(memory);
            }
        }
    }
}
