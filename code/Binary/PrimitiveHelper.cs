using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary
{
    public static class PrimitiveHelper
    {
        public static void EncodeNumber(ref Allocator allocator, int number)
        {
            if (number < 0)
                ThrowHelper.ThrowNumberNegative();
            var numberLength = MemoryHelper.EncodeNumberLength((uint)number);
            MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
        }

        public static int DecodeNumber(ref ReadOnlySpan<byte> span)
        {
            ref var source = ref MemoryHelper.EnsureLength(span);
            var numberLength = MemoryHelper.DecodeNumberLength(source);
            // check bounds via slice method
            span = span.Slice(numberLength);
            return MemoryHelper.DecodeNumber(ref source, numberLength);
        }

        public static void EncodeBufferWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<byte> span)
        {
            var length = span.Length;
            var numberLength = MemoryHelper.EncodeNumberLength((uint)length);
            MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)length, numberLength);
            Allocator.Append(ref allocator, span);
        }

        public static ReadOnlySpan<byte> DecodeBufferWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            var data = span;
            ref var source = ref MemoryHelper.EnsureLength(data);
            var numberLength = MemoryHelper.DecodeNumberLength(source);
            // check bounds via slice method
            data = data.Slice(numberLength);
            var length = MemoryHelper.DecodeNumber(ref source, numberLength);
            // check bounds via slice method
            span = data.Slice(length);
            return data.Slice(0, length);
        }

        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            Allocator.AppendString(ref allocator, span);
        }

        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            Allocator.AppendStringWithLengthPrefix(ref allocator, span);
        }

        public static string DecodeString(ReadOnlySpan<byte> span)
        {
            return SharedHelper.Encoding.GetString(span);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return SharedHelper.Encoding.GetString(DecodeBufferWithLengthPrefix(ref span));
        }
    }
}
