using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Extensions;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        public static void EncodeString(ref Allocator allocator, in ReadOnlySpan<char> span, Encoding encoding) => allocator.AppendText(in span, encoding, lengthPrefix: false);

        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, in ReadOnlySpan<char> span, Encoding encoding) => allocator.AppendText(in span, encoding, lengthPrefix: true);

        public static string DecodeString(in ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding == null)
                ThrowHelper.ThrowArgumentNull(nameof(encoding));
            return encoding.GetString(in span);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding == null)
                ThrowHelper.ThrowArgumentNull(nameof(encoding));
            var spanLength = span.Length;
            if (spanLength == 0)
                goto fail;
            ref var location = ref MemoryMarshal.GetReference(span);
            var prefixLength = DecodePrefixLength(location);
            if (spanLength < prefixLength)
                goto fail;
            var length = DecodeLengthPrefix(ref location, prefixLength);
            if ((uint)spanLength < (uint)(prefixLength + length))
                goto fail;
            var result = encoding.GetString(ref Memory.Add(ref location, prefixLength), length);
            span = span.Slice(prefixLength + length);
            return result;

        fail:
            return ThrowHelper.ThrowLengthPrefixInvalidBytes<string>();
        }
    }
}
