using Mikodev.Binary.Abstractions;
using System;

namespace Mikodev.Binary.Creators.Primitives
{
    internal sealed class StringConverter : VariableConverter<string>
    {
        public override void Encode(ref Allocator allocator, string item) => PrimitiveHelper.EncodeString(ref allocator, item.AsSpan());

        public override void EncodeWithLengthPrefix(ref Allocator allocator, string item) => PrimitiveHelper.EncodeStringWithLengthPrefix(ref allocator, item.AsSpan());

        public override string Decode(in ReadOnlySpan<byte> span) => PrimitiveHelper.DecodeString(in span);

        public override string DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => PrimitiveHelper.DecodeStringWithLengthPrefix(ref span);
    }
}
