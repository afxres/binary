namespace Mikodev.Binary.Features.Contexts;

internal abstract class VariablePrefixEncodeConverter<T, U> : VariableConverter<T, U> where U : struct, IVariablePrefixEncodeConverterFunctions<T>
{
    public override void Encode(ref Allocator allocator, T? item) => U.Encode(ref allocator, item);

    public override void EncodeAuto(ref Allocator allocator, T? item) => U.EncodeWithLengthPrefix(ref allocator, item);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item) => U.EncodeWithLengthPrefix(ref allocator, item);
}
