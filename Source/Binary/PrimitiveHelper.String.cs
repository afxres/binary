using Mikodev.Binary.Internal;
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
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));
            var charCount = span.Length;
            ref var chars = ref MemoryMarshal.GetReference(span);
            var byteCount = charCount == 0 ? 0 : encoding.GetByteCount(ref chars, charCount);
            if (withLengthPrefix)
                EncodeNumber(ref allocator, byteCount);
            if (byteCount == 0)
                return;
            ref var bytes = ref Allocator.AllocateReference(ref allocator, byteCount);
            _ = encoding.GetBytes(ref bytes, byteCount, ref chars, charCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeString(ref Allocator allocator, in ReadOnlySpan<char> span, Encoding encoding) => Encode(ref allocator, in span, encoding, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, in ReadOnlySpan<char> span, Encoding encoding) => Encode(ref allocator, in span, encoding, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeString(ref Allocator allocator, in ReadOnlySpan<char> span) => Allocator.Encode(ref allocator, in span, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, in ReadOnlySpan<char> span) => Allocator.Encode(ref allocator, in span, true);

        public static string DecodeString(in ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));
            return encoding.GetString(in span);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));
            return encoding.GetString(DecodeBufferWithLengthPrefix(ref span));
        }

        public static string DecodeString(in ReadOnlySpan<byte> span)
        {
            return StringHelper.Decode(Converter.Encoding, ref MemoryMarshal.GetReference(span), span.Length);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            var encoding = Converter.Encoding;
            var limits = span.Length;
            if (limits == 0)
                goto fail;
            ref var location = ref MemoryMarshal.GetReference(span);
            var prefixLength = DecodeNumberLength(location);
            if (limits < prefixLength)
                goto fail;
            var length = DecodeNumber(ref location, prefixLength);
            // check bounds via slice method
            span = span.Slice(prefixLength + length);
            return StringHelper.Decode(encoding, ref MemoryHelper.Add(ref location, prefixLength), length);

        fail:
            ThrowHelper.ThrowNotEnoughBytes();
            throw null;
        }
    }
}
