namespace Mikodev.Binary.Components;

using Mikodev.Binary.Internal;
using System;

public abstract class TupleObjectConverter<T> : Converter<T>
{
    protected TupleObjectConverter(int length) : base(length) { }

    public abstract override void Encode(ref Allocator allocator, T? item);

    public abstract override void EncodeAuto(ref Allocator allocator, T? item);

    public override T Decode(in ReadOnlySpan<byte> span) => ThrowHelper.ThrowNoSuitableConstructor<T>();

    public override T DecodeAuto(ref ReadOnlySpan<byte> span) => ThrowHelper.ThrowNoSuitableConstructor<T>();
}
