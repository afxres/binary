using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Converters
{
    internal sealed class UriConverter : Converter<Uri>
    {
        private static ReadOnlySpan<char> EncodeInternal(Uri item) => (item?.OriginalString).AsSpan();

        private static Uri DecodeInternal(string item) => string.IsNullOrEmpty(item) ? null : new Uri(item);

        public override void Encode(ref Allocator allocator, Uri item) => Allocator.Append(ref allocator, EncodeInternal(item), SharedHelper.Encoding);

        public override void EncodeAuto(ref Allocator allocator, Uri item) => Allocator.AppendWithLengthPrefix(ref allocator, EncodeInternal(item), SharedHelper.Encoding);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, Uri item) => Allocator.AppendWithLengthPrefix(ref allocator, EncodeInternal(item), SharedHelper.Encoding);

        public override Uri Decode(in ReadOnlySpan<byte> span) => DecodeInternal(SharedHelper.Encoding.GetString(span));

        public override Uri DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeInternal(SharedHelper.Encoding.GetString(Converter.DecodeWithLengthPrefix(ref span)));

        public override Uri DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeInternal(SharedHelper.Encoding.GetString(Converter.DecodeWithLengthPrefix(ref span)));
    }
}
