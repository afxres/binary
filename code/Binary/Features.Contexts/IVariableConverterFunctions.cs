namespace Mikodev.Binary.Features.Contexts;

using System;

internal interface IVariableConverterFunctions<T>
{
    static abstract T Decode(in ReadOnlySpan<byte> span);
}
