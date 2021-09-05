namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

public abstract partial class Converter<T>
{
    public virtual T Decode(byte[] buffer)
    {
        return Decode(new ReadOnlySpan<byte>(buffer));
    }

    public virtual T DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        var length = this.length;
        if (length is not 0)
            return Decode(MemoryMarshal.CreateReadOnlySpan(ref MemoryHelper.EnsureLength(ref span, length), length));
        else
            return Decode(Converter.DecodeWithLengthPrefix(ref span));
    }

    public virtual T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
    {
        return Decode(Converter.DecodeWithLengthPrefix(ref span));
    }

    public virtual byte[] Encode(T item)
    {
        var length = this.length;
        if (length is not 0)
            return EncodeConstant(item);
        else
            return EncodeVariable(item);
    }

    public virtual void EncodeAuto(ref Allocator allocator, T item)
    {
        var length = this.length;
        if (length is not 0)
            Encode(ref allocator, item);
        else
            EncodeWithLengthPrefixVariable(ref allocator, item);
    }

    public virtual void EncodeWithLengthPrefix(ref Allocator allocator, T item)
    {
        var length = this.length;
        if (length is not 0)
            EncodeWithLengthPrefixConstant(ref allocator, item);
        else
            EncodeWithLengthPrefixVariable(ref allocator, item);
    }
}
