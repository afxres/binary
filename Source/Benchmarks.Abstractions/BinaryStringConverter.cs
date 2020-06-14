using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Benchmarks.Abstractions
{
    public class BinaryStringConverter : Converter<string>
    {
        public override void Encode(ref Allocator allocator, string item) => AllocatorHelper.Append(ref allocator, MemoryMarshal.Cast<char, byte>(item.AsSpan()));

        public override string Decode(in ReadOnlySpan<byte> span) => MemoryMarshal.Cast<byte, char>(span).ToString();

        public override void EncodeAuto(ref Allocator allocator, string item) => PrimitiveHelper.EncodeBufferWithLengthPrefix(ref allocator, MemoryMarshal.Cast<char, byte>(item.AsSpan()));

        public override string DecodeAuto(ref ReadOnlySpan<byte> span) => MemoryMarshal.Cast<byte, char>(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span)).ToString();

        public override void EncodeWithLengthPrefix(ref Allocator allocator, string item) => PrimitiveHelper.EncodeBufferWithLengthPrefix(ref allocator, MemoryMarshal.Cast<char, byte>(item.AsSpan()));

        public override string DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => MemoryMarshal.Cast<byte, char>(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span)).ToString();
    }
}
