namespace Mikodev.Binary.Benchmarks.Abstractions;

using System;
using System.Runtime.InteropServices;

public class BinaryStringConverter : Converter<string>
{
    public override string Decode(in ReadOnlySpan<byte> span) => MemoryMarshal.Cast<byte, char>(span).ToString();

    public override string DecodeAuto(ref ReadOnlySpan<byte> span) => MemoryMarshal.Cast<byte, char>(Converter.DecodeWithLengthPrefix(ref span)).ToString();

    public override string DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => MemoryMarshal.Cast<byte, char>(Converter.DecodeWithLengthPrefix(ref span)).ToString();

    public override void Encode(ref Allocator allocator, string? item) => Allocator.Append(ref allocator, MemoryMarshal.Cast<char, byte>(item.AsSpan()));

    public override void EncodeAuto(ref Allocator allocator, string? item) => Converter.EncodeWithLengthPrefix(ref allocator, MemoryMarshal.Cast<char, byte>(item.AsSpan()));

    public override void EncodeWithLengthPrefix(ref Allocator allocator, string? item) => Converter.EncodeWithLengthPrefix(ref allocator, MemoryMarshal.Cast<char, byte>(item.AsSpan()));
}
