using Mikodev.Binary.Abstractions;
using System;

namespace Mikodev.Binary.Creators.Primitives
{
    internal sealed class StringConverter : VariableConverter<string>
    {
        public override void ToBytes(ref Allocator allocator, string item) => PrimitiveHelper.EncodeString(ref allocator, item.AsSpan());

        public override void ToBytesWithLengthPrefix(ref Allocator allocator, string item) => PrimitiveHelper.EncodeStringWithLengthPrefix(ref allocator, item.AsSpan());

        public override string ToValue(in ReadOnlySpan<byte> span) => PrimitiveHelper.DecodeString(in span);

        public override string ToValueWithLengthPrefix(ref ReadOnlySpan<byte> span) => PrimitiveHelper.DecodeStringWithLengthPrefix(ref span);
    }
}
