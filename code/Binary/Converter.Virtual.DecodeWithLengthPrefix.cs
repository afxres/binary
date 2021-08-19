namespace Mikodev.Binary;

using System;

public abstract partial class Converter<T>
{
    public virtual T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
    {
        return Decode(Converter.DecodeWithLengthPrefix(ref span));
    }
}
