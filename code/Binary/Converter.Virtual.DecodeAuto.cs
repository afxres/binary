namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

public abstract partial class Converter<T>
{
    public virtual T DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        var length = this.length;
        if (length is not 0)
            return Decode(MemoryMarshal.CreateReadOnlySpan(ref MemoryHelper.EnsureLength(ref span, length), length));
        else
            return Decode(Converter.DecodeWithLengthPrefix(ref span));
    }
}
