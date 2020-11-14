using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary
{
    public static class PrimitiveHelper
    {
        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            Allocator.Append(ref allocator, span, SharedHelper.Encoding);
        }

        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            Allocator.AppendWithLengthPrefix(ref allocator, span, SharedHelper.Encoding);
        }

        public static string DecodeString(ReadOnlySpan<byte> span)
        {
            return SharedHelper.Encoding.GetString(span);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return SharedHelper.Encoding.GetString(Converter.DecodeWithLengthPrefix(ref span));
        }
    }
}
