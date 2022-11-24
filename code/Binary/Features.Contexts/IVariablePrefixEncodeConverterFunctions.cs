namespace Mikodev.Binary.Features.Contexts;

internal interface IVariablePrefixEncodeConverterFunctions<T> : IVariableDirectEncodeConverterFunctions<T>
{
    static abstract void EncodeWithLengthPrefix(ref Allocator allocator, T? item);
}
