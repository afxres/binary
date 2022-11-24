namespace Mikodev.Binary.Features.Contexts;

internal interface IVariableDirectEncodeConverterFunctions<T> : IVariableConverterFunctions<T>
{
    static abstract void Encode(ref Allocator allocator, T? item);
}
