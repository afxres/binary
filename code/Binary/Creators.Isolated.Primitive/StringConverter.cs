namespace Mikodev.Binary.Creators.Isolated.Primitive;

using System;
using System.Text;

internal sealed class StringConverter : Converter<string>
{
    public override void Encode(ref Allocator allocator, string? item) => Allocator.Append(ref allocator, item.AsSpan(), Encoding.UTF8);

    public override void EncodeAuto(ref Allocator allocator, string? item) => Allocator.AppendWithLengthPrefix(ref allocator, item.AsSpan(), Encoding.UTF8);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, string? item) => Allocator.AppendWithLengthPrefix(ref allocator, item.AsSpan(), Encoding.UTF8);

    public override string Decode(in ReadOnlySpan<byte> span) => Encoding.UTF8.GetString(span);

    public override string DecodeAuto(ref ReadOnlySpan<byte> span) => Encoding.UTF8.GetString(Converter.DecodeWithLengthPrefix(ref span));

    public override string DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => Encoding.UTF8.GetString(Converter.DecodeWithLengthPrefix(ref span));
}
