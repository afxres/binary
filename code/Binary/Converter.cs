namespace Mikodev.Binary;

using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Metadata;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public abstract partial class Converter<T>
{
    private readonly int length;

    public int Length => this.length;

    protected Converter() : this(0) { }

    protected Converter(int length)
    {
        if (length < 0)
            ThrowHelper.ThrowLengthNegative();
        this.length = length;
        EnsureOverride<DecodeDelegate<T>>(DecodeAuto, DecodeWithLengthPrefix);
        EnsureOverride<EncodeDelegate<T>>(EncodeAuto, EncodeWithLengthPrefix);
    }

    public abstract void Encode(ref Allocator allocator, T item);

    public abstract T Decode(in ReadOnlySpan<byte> span);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object obj) => ReferenceEquals(this, obj);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString() => $"{nameof(Converter<T>)}<{typeof(T).Name}>({nameof(Length)}: {Length})";
}
