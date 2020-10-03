using Mikodev.Binary.Internal;
using System;
using System.Buffers;
using System.Runtime.InteropServices;

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
            Allocator.Append(ref allocator, span);
        }

        public static void Append<T>(ref Allocator allocator, AllocatorAnchor anchor, T data, SpanAction<byte, T> action)
        {
            if (action is null)
                ThrowHelper.ThrowActionNull();
            // check bounds via slice method
            var target = allocator.AsSpan().Slice(anchor.Offset, anchor.Length);
            var length = target.Length;
            if (length == 0)
                return;
            action.Invoke(MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(target), length), data);
        }

        public static void Append<T>(ref Allocator allocator, int length, T data, SpanAction<byte, T> action)
        {
            if (action is null)
                ThrowHelper.ThrowActionNull();
            if (length == 0)
                return;
            action.Invoke(MemoryMarshal.CreateSpan(ref Allocator.Assign(ref allocator, length), length), data);
        }

        public static void AppendWithLengthPrefix<T>(ref Allocator allocator, T data, AllocatorAction<T> action)
        {
            if (action is null)
                ThrowHelper.ThrowActionNull();
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            action.Invoke(ref allocator, data);
            Allocator.AppendLengthPrefix(ref allocator, anchor);
        }

        public static void Ensure(ref Allocator allocator, int length)
        {
            Allocator.Ensure(ref allocator, length);
        }

        public static void Expand(ref Allocator allocator, int length)
        {
            Allocator.Expand(ref allocator, length);
        }

        public static byte[] Invoke<T>(T data, AllocatorAction<T> action)
        {
            if (action is null)
                ThrowHelper.ThrowActionNull();
            var handle = BufferHelper.Borrow();
            try
            {
                var allocator = new Allocator(BufferHelper.Result(handle));
                action.Invoke(ref allocator, data);
                return Allocator.Result(ref allocator);
            }
            finally
            {
                BufferHelper.Return(handle);
            }
        }
    }
}
