namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;

public abstract partial class Converter<T>
{
    public virtual T DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        var length = this.length;
        if (length is not 0)
            return Decode(MemoryHelper.EnsureLengthReturnBuffer(ref span, this.length));
        else
            return Decode(Converter.DecodeWithLengthPrefix(ref span));
    }
}
