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
                ThrowHelper.ThrowArgumentEncodingNull();
            Allocator.AppendString(ref allocator, span, encoding);
        }

        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingNull();
            Allocator.AppendStringWithLengthPrefix(ref allocator, span, encoding);
        }

        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            Allocator.AppendString(ref allocator, span, SharedHelper.Encoding);
        }

        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            Allocator.AppendStringWithLengthPrefix(ref allocator, span, SharedHelper.Encoding);
        }

        public static string DecodeString(ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingNull();
            return SharedHelper.GetString(span, encoding);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingNull();
            return SharedHelper.GetString(DecodeBufferWithLengthPrefix(ref span), encoding);
        }

        public static string DecodeString(ReadOnlySpan<byte> span)
        {
            return SharedHelper.GetString(span, SharedHelper.Encoding);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return SharedHelper.GetString(DecodeBufferWithLengthPrefix(ref span), SharedHelper.Encoding);
        }
    }
}
