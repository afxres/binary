namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;

public abstract partial class Converter<T>
{
    public virtual T DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        var option = this.decode;
        if (option is DecodeOption.Constant)
            return Decode(MemoryHelper.EnsureLengthReturnBuffer(ref span, this.length));
        else if (option is DecodeOption.Variable)
            return Decode(Converter.DecodeWithLengthPrefix(ref span));
        else
            return DecodeWithLengthPrefix(ref span);
    }
}
