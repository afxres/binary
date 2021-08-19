namespace Mikodev.Binary;

using System;

public abstract partial class Converter<T>
{
    public virtual T DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        var option = this.decode;
        if (option is DecodeOption.Constant)
            return DecodeAutoConstant(ref span);
        else if (option is DecodeOption.Variable)
            return DecodeAutoVariable(ref span);
        else
            return DecodeWithLengthPrefix(ref span);
    }
}
