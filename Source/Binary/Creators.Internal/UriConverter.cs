using System;

namespace Mikodev.Binary.Creators.Internal
{
    internal sealed class UriConverter : Converter<Uri>
    {
        private static ReadOnlySpan<char> Of(Uri item) => (item?.OriginalString).AsSpan();

        private static void Append(ref Allocator allocator, Uri item) => PrimitiveHelper.EncodeString(ref allocator, Of(item));

        private static void AppendWithLengthPrefix(ref Allocator allocator, Uri item) => PrimitiveHelper.EncodeStringWithLengthPrefix(ref allocator, Of(item));

        private static Uri To(string item) => string.IsNullOrEmpty(item) ? null : new Uri(item);

        private static Uri Detach(ReadOnlySpan<byte> span) => To(PrimitiveHelper.DecodeString(span));

        private static Uri DetachWithLengthPrefix(ref ReadOnlySpan<byte> span) => To(PrimitiveHelper.DecodeStringWithLengthPrefix(ref span));

        public override void Encode(ref Allocator allocator, Uri item) => Append(ref allocator, item);

        public override void EncodeAuto(ref Allocator allocator, Uri item) => AppendWithLengthPrefix(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, Uri item) => AppendWithLengthPrefix(ref allocator, item);

        public override Uri Decode(in ReadOnlySpan<byte> span) => Detach(span);

        public override Uri DecodeAuto(ref ReadOnlySpan<byte> span) => DetachWithLengthPrefix(ref span);

        public override Uri DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DetachWithLengthPrefix(ref span);
    }
}
