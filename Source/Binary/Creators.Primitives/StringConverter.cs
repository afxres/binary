using Mikodev.Binary.Abstractions;
using System;

namespace Mikodev.Binary.Creators.Primitives
{
    internal sealed class StringConverter : VariableConverter<string>
    {
        public override void ToBytes(ref Allocator allocator, string item) => PrimitiveHelper.EncodeString(ref allocator, item.AsSpan(), Encoding);

        public override void ToBytesWithLengthPrefix(ref Allocator allocator, string item) => PrimitiveHelper.EncodeStringWithLengthPrefix(ref allocator, item.AsSpan(), Encoding);

        public override string ToValue(in ReadOnlySpan<byte> span) => PrimitiveHelper.DecodeString(in span, Encoding);

        public override string ToValueWithLengthPrefix(ref ReadOnlySpan<byte> span) => PrimitiveHelper.DecodeStringWithLengthPrefix(ref span, Encoding);
    }
}
