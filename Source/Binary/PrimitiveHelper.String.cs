using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        private static void AppendString(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            Allocator.AppendString(ref allocator, ref MemoryMarshal.GetReference(span), span.Length, encoding);
        }

        private static void AppendStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            Allocator.AppendStringWithLengthPrefix(ref allocator, ref MemoryMarshal.GetReference(span), span.Length, encoding);
        }

        private static string DetachString(ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            return StringHelper.GetString(encoding, ref MemoryMarshal.GetReference(span), span.Length);
        }

        private static string DetachStringWithLengthPrefix(ref ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            var data = DecodeBufferWithLengthPrefix(ref span);
            return StringHelper.GetString(encoding, ref MemoryMarshal.GetReference(data), data.Length);
        }

        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            AppendString(ref allocator, span, encoding);
        }

        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            AppendStringWithLengthPrefix(ref allocator, span, encoding);
        }

        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            AppendString(ref allocator, span, Converter.Encoding);
        }

        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            AppendStringWithLengthPrefix(ref allocator, span, Converter.Encoding);
        }

        public static string DecodeString(ReadOnlySpan<byte> span, Encoding encoding)
        {
            return DetachString(span, encoding);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span, Encoding encoding)
        {
            return DetachStringWithLengthPrefix(ref span, encoding);
        }

        public static string DecodeString(ReadOnlySpan<byte> span)
        {
            return DetachString(span, Converter.Encoding);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return DetachStringWithLengthPrefix(ref span, Converter.Encoding);
        }
    }
}
