namespace Mikodev.Binary.Features.Contexts;

using System;

internal interface IVariableConverterFunctions<T>
{
    static abstract T Decode(in ReadOnlySpan<byte> span);

    static abstract void Encode(ref Allocator allocator, T? item);

    static abstract void EncodeWithLengthPrefix(ref Allocator allocator, T? item);
}
