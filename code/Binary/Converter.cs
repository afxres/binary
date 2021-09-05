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
        var parent = typeof(Converter<T>);
        if (new DecodeDelegate<T>(DecodeAuto).Method.DeclaringType == parent && new DecodeDelegate<T>(DecodeWithLengthPrefix).Method.DeclaringType != parent)
            ThrowHelper.ThrowNotOverride(nameof(DecodeAuto), nameof(DecodeWithLengthPrefix), GetType());
        if (new EncodeDelegate<T>(EncodeAuto).Method.DeclaringType == parent && new EncodeDelegate<T>(EncodeWithLengthPrefix).Method.DeclaringType != parent)
            ThrowHelper.ThrowNotOverride(nameof(EncodeAuto), nameof(EncodeWithLengthPrefix), GetType());
        this.length = length;
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
