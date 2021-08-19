namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Metadata;
using System;

public abstract partial class Converter<T>
{
    private enum DecodeOption { Constant, Variable, VariableOverride };

    private DecodeOption DecodeOptionInternal()
    {
        var length = this.length;
        if (length is not 0)
            return DecodeOption.Constant;
        var method = new DecodeDelegate<T>(DecodeWithLengthPrefix).Method;
        if (method.DeclaringType == typeof(Converter<T>))
            return DecodeOption.Variable;
        else
            return DecodeOption.VariableOverride;
    }

    private T DecodeAutoConstant(ref ReadOnlySpan<byte> span)
    {
        return Decode(MemoryHelper.EnsureLengthReturnBuffer(ref span, this.length));
    }

    private T DecodeAutoVariable(ref ReadOnlySpan<byte> span)
    {
        return Decode(Converter.DecodeWithLengthPrefix(ref span));
    }
}
