namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Internal;
using System;

internal sealed class StringConverter : Converter<string>
{
    public override void Encode(ref Allocator allocator, string? item) => Allocator.Append(ref allocator, item.AsSpan(), SharedHelper.Encoding);

    public override void EncodeAuto(ref Allocator allocator, string? item) => Allocator.AppendWithLengthPrefix(ref allocator, item.AsSpan(), SharedHelper.Encoding);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, string? item) => Allocator.AppendWithLengthPrefix(ref allocator, item.AsSpan(), SharedHelper.Encoding);

    public override string Decode(in ReadOnlySpan<byte> span) => SharedHelper.Encoding.GetString(span);

    public override string DecodeAuto(ref ReadOnlySpan<byte> span) => SharedHelper.Encoding.GetString(Converter.DecodeWithLengthPrefix(ref span));

    public override string DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => SharedHelper.Encoding.GetString(Converter.DecodeWithLengthPrefix(ref span));
}
