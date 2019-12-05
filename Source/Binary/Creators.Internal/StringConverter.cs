using System;

namespace Mikodev.Binary.Creators.Internal
{
    internal sealed class StringConverter : Converter<string>
    {
        public override void Encode(ref Allocator allocator, string item) => PrimitiveHelper.EncodeString(ref allocator, item.AsSpan());

        public override void EncodeAuto(ref Allocator allocator, string item) => PrimitiveHelper.EncodeStringWithLengthPrefix(ref allocator, item.AsSpan());

        public override void EncodeWithLengthPrefix(ref Allocator allocator, string item) => PrimitiveHelper.EncodeStringWithLengthPrefix(ref allocator, item.AsSpan());

        public override string Decode(in ReadOnlySpan<byte> span) => PrimitiveHelper.DecodeString(span);

        public override string DecodeAuto(ref ReadOnlySpan<byte> span) => PrimitiveHelper.DecodeStringWithLengthPrefix(ref span);

        public override string DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => PrimitiveHelper.DecodeStringWithLengthPrefix(ref span);
    }
}
