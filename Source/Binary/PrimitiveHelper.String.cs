using Mikodev.Binary.Internal;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            Allocator.AppendStringEncoding(ref allocator, ref MemoryMarshal.GetReference(span), span.Length, encoding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span, Encoding encoding)
        {
            Allocator.AppendStringEncodingWithLengthPrefix(ref allocator, ref MemoryMarshal.GetReference(span), span.Length, encoding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeString(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            Allocator.AppendString(ref allocator, ref MemoryMarshal.GetReference(span), span.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeStringWithLengthPrefix(ref Allocator allocator, ReadOnlySpan<char> span)
        {
            Allocator.AppendStringWithLengthPrefix(ref allocator, ref MemoryMarshal.GetReference(span), span.Length);
        }

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
