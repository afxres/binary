namespace Mikodev.Binary;

using System;

public abstract partial class Converter<T>
{
    public virtual T Decode(byte[] buffer)
    {
        return Decode(new ReadOnlySpan<byte>(buffer));
    }
}
