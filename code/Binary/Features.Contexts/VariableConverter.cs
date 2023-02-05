namespace Mikodev.Binary.Features.Contexts;

using System;

internal abstract class VariableConverter<T, U> : Converter<T> where U : struct, IVariableConverterFunctions<T>
{
    public override T Decode(in ReadOnlySpan<byte> span) => U.Decode(in span);

    public override T DecodeAuto(ref ReadOnlySpan<byte> span) => U.Decode(Converter.DecodeWithLengthPrefix(ref span));

    public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => U.Decode(Converter.DecodeWithLengthPrefix(ref span));

    public override void Encode(ref Allocator allocator, T? item) => U.Encode(ref allocator, item);

    public override void EncodeAuto(ref Allocator allocator, T? item) => U.EncodeWithLengthPrefix(ref allocator, item);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item) => U.EncodeWithLengthPrefix(ref allocator, item);
}
