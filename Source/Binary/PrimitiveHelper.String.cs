using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Extensions;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        private static void Encode(ref Allocator allocator, in ReadOnlySpan<char> span, Encoding encoding, bool withLengthPrefix)
        {
            if (encoding == null)
                ThrowHelper.ThrowArgumentNull(nameof(encoding));
            var charCount = span.Length;
            ref var chars = ref MemoryMarshal.GetReference(span);
            var byteCount = charCount == 0 ? 0 : encoding.GetByteCount(ref chars, charCount);
            if (withLengthPrefix)
                EncodeLengthPrefix(ref allocator, (uint)byteCount);
            if (byteCount == 0)
                return;
            var bytes = allocator.AllocateReference(byteCount);
            _ = encoding.GetBytes(ref bytes, byteCount, ref chars, charCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeString(ref Allocator allocator, in ReadOnlySpan<char> span, Encoding encoding) => Encode(ref allocator, in span, encoding, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, in ReadOnlySpan<char> span, Encoding encoding) => Encode(ref allocator, in span, encoding, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeString(ref Allocator allocator, in ReadOnlySpan<char> span) => allocator.Encode(in span, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, in ReadOnlySpan<char> span) => allocator.Encode(in span, true);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DecodeString(in ReadOnlySpan<byte> span) => DecodeString(in span, Converter.Encoding);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeStringWithLengthPrefix(ref span, Converter.Encoding);
    }
}
