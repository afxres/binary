namespace Mikodev.Binary.Features.Contexts;

using System;

internal interface IVariableWriterEncodeConverterFunctions<T> : IVariableConverterFunctions<T>
{
    static abstract int GetMaxLength(T? item);

    static abstract int Encode(Span<byte> span, T? item);
}
