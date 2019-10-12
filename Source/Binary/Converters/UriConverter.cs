using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal.Extensions;
using System;

namespace Mikodev.Binary.Converters
{
    internal sealed class UriConverter : VariableConverter<Uri>
    {
        public override void ToBytes(ref Allocator allocator, Uri item) => allocator.Append((item?.OriginalString).AsSpan(), Encoding);

        public override void ToBytesWithLengthPrefix(ref Allocator allocator, Uri item) => allocator.AppendWithLengthPrefix((item?.OriginalString).AsSpan(), Encoding);

        public override Uri ToValue(in ReadOnlySpan<byte> span) => span.IsEmpty ? null : new Uri(Encoding.GetString(in span));
    }
}
