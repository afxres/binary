using Mikodev.Binary.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
            var limits = span.Length;
            var offset = 0;
            var length = MemoryHelper.DecodeNumber(ref source, ref offset, limits);
            span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, offset), limits - offset);
            return length;
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
            ref var source = ref MemoryHelper.EnsureLength(span);
            var limits = span.Length;
            var offset = 0;
            var length = MemoryHelper.DecodeNumberEnsureBuffer(ref source, ref offset, limits);
            var cursor = offset + length;
            span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, cursor), limits - cursor);
            return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, offset), length);
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
