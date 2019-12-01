using Mikodev.Binary.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        private static void Encode(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding, bool withLengthPrefix)
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
            _ = encoding.GetBytes(ref Allocator.Assign(ref allocator, byteCount), byteCount, ref chars, charCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding) => Encode(ref allocator, span, encoding, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding) => Encode(ref allocator, span, encoding, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span) => Allocator.AppendString(ref allocator, span, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span) => Allocator.AppendString(ref allocator, span, true);

        public static string DecodeString(ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));
            return encoding.GetString(ref MemoryMarshal.GetReference(span), span.Length);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (encoding is null)
                throw new ArgumentNullException(nameof(encoding));
            var data = DecodeBufferWithLengthPrefix(ref span);
            return encoding.GetString(ref MemoryMarshal.GetReference(data), data.Length);
        }

        public static string DecodeString(ReadOnlySpan<byte> span)
        {
            return StringHelper.GetString(Converter.Encoding, ref MemoryMarshal.GetReference(span), span.Length);
        }

        public static string DecodeStringWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            int numberLength;
            var limits = span.Length;
            ref var source = ref MemoryMarshal.GetReference(span);
            if (limits == 0 || limits < (numberLength = DecodeNumberLength(source)))
                return ThrowHelper.ThrowNotEnoughBytes<string>();
            var length = DecodeNumber(ref source, numberLength);
            // check bounds via slice method
            span = span.Slice(numberLength).Slice(length);
            return StringHelper.GetString(Converter.Encoding, ref Unsafe.Add(ref source, numberLength), length);
        }
    }
}
