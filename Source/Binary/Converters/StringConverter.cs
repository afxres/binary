using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal.Extensions;
using System;

namespace Mikodev.Binary.Converters
{
    internal sealed class StringConverter : VariableConverter<string>
    {
        public override void ToBytes(ref Allocator allocator, string item) => allocator.Append(item.AsSpan(), Encoding);

        public override void ToBytesWithLengthPrefix(ref Allocator allocator, string item) => allocator.AppendWithLengthPrefix(item.AsSpan(), Encoding);

        public override string ToValue(in ReadOnlySpan<byte> span) => Encoding.GetString(in span);
    }
}
