using System;

namespace Mikodev.Binary.Converters
{
    internal sealed class UriConverter : Converter<Uri>
    {
        private static ReadOnlySpan<char> EncodeUri(Uri item) => (item?.OriginalString).AsSpan();

        private static void EncodeInternal(ref Allocator allocator, Uri item) => PrimitiveHelper.EncodeString(ref allocator, EncodeUri(item));

        private static void EncodeWithLengthPrefixInternal(ref Allocator allocator, Uri item) => PrimitiveHelper.EncodeStringWithLengthPrefix(ref allocator, EncodeUri(item));

        private static Uri DecodeUri(string item) => string.IsNullOrEmpty(item) ? null : new Uri(item);

        private static Uri DecodeInternal(ReadOnlySpan<byte> span) => DecodeUri(PrimitiveHelper.DecodeString(span));

        private static Uri DecodeWithLengthPrefixInternal(ref ReadOnlySpan<byte> span) => DecodeUri(PrimitiveHelper.DecodeStringWithLengthPrefix(ref span));

        public override void Encode(ref Allocator allocator, Uri item) => EncodeInternal(ref allocator, item);

        public override void EncodeAuto(ref Allocator allocator, Uri item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, Uri item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override Uri Decode(in ReadOnlySpan<byte> span) => DecodeInternal(span);

        public override Uri DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeWithLengthPrefixInternal(ref span);

        public override Uri DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeWithLengthPrefixInternal(ref span);
    }
}
