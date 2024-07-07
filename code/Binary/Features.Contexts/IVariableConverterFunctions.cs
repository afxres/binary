namespace Mikodev.Binary.Features.Contexts;

using System;

internal interface IVariableConverterFunctions<T>
{
    static abstract int Limits(T? item);

    static abstract int Append(Span<byte> span, T? item);

    static abstract T Decode(in ReadOnlySpan<byte> span);
}
