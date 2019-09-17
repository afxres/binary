using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Converters
{
    internal sealed class UriConverter : VariableConverter<Uri>
    {
        public override void ToBytes(ref Allocator allocator, Uri item) => Format.SetText(ref allocator, item?.OriginalString);

        public override Uri ToValue(in ReadOnlySpan<byte> span) => span.IsEmpty ? null : new Uri(Format.GetText(in span));
    }
}
