using Mikodev.Binary.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        private static unsafe string DetachString(ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            fixed (byte* srcptr = &MemoryMarshal.GetReference(span))
                return StringHelper.GetString(encoding, srcptr, span.Length);
        }

        private static unsafe string DetachStringWithLengthPrefix(ref ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            int numberLength;
            var limits = span.Length;
            ref var source = ref MemoryMarshal.GetReference(span);
            if (limits == 0 || limits < (numberLength = DecodeNumberLength(source)))
                return ThrowHelper.ThrowNotEnoughBytes<string>();
            var length = DecodeNumber(ref source, numberLength);
            // check bounds via slice method
            span = span.Slice(numberLength).Slice(length);
            fixed (byte* srcptr = &source)
                return StringHelper.GetString(encoding, srcptr + numberLength, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            Allocator.AppendString(ref allocator, ref MemoryMarshal.GetReference(span), span.Length, encoding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            Allocator.AppendStringWithLengthPrefix(ref allocator, ref MemoryMarshal.GetReference(span), span.Length, encoding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            Allocator.AppendString(ref allocator, ref MemoryMarshal.GetReference(span), span.Length, Converter.Encoding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            Allocator.AppendStringWithLengthPrefix(ref allocator, ref MemoryMarshal.GetReference(span), span.Length, Converter.Encoding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DecodeString(ReadOnlySpan<byte> span, Encoding encoding)
        {
            return DetachString(span, encoding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span, Encoding encoding)
        {
            return DetachStringWithLengthPrefix(ref span, encoding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DecodeString(ReadOnlySpan<byte> span)
        {
            return DetachString(span, Converter.Encoding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return DetachStringWithLengthPrefix(ref span, Converter.Encoding);
        }
    }
}
