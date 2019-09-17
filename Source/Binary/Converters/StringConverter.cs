using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Converters
{
    internal sealed class StringConverter : VariableConverter<string>
    {
        public override void ToBytes(ref Allocator allocator, string item) => Format.SetText(ref allocator, item);

        public override string ToValue(in ReadOnlySpan<byte> span) => Format.GetText(in span);
    }
}
