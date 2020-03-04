using Mikodev.Binary.Internal;
using System;
using System.Text;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            Allocator.AppendString(ref allocator, span, encoding);
        }

        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            Allocator.AppendStringWithLengthPrefix(ref allocator, span, encoding);
        }

        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            Allocator.AppendString(ref allocator, span, Converter.Encoding);
        }

        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            Allocator.AppendStringWithLengthPrefix(ref allocator, span, Converter.Encoding);
        }

        public static string DecodeString(ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            return SharedHelper.GetString(span, encoding);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            return SharedHelper.GetString(DecodeBufferWithLengthPrefix(ref span), encoding);
        }

        public static string DecodeString(ReadOnlySpan<byte> span)
        {
            return SharedHelper.GetString(span, Converter.Encoding);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return SharedHelper.GetString(DecodeBufferWithLengthPrefix(ref span), Converter.Encoding);
        }
    }
}
