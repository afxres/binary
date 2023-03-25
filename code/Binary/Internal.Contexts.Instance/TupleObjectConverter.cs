namespace Mikodev.Binary.Internal.Contexts.Instance;

using Mikodev.Binary.Internal.Metadata;
using System;
using System.Diagnostics;

internal sealed class TupleObjectConverter<T> : Converter<T>
{
    private readonly EncodeDelegate<T> encode;

    private readonly EncodeDelegate<T> encodeAuto;

    private readonly DecodeDelegate<T> decode;

    private readonly DecodeDelegate<T> decodeAuto;

    public TupleObjectConverter(EncodeDelegate<T> encode, EncodeDelegate<T> encodeAuto, DecodeDelegate<T>? decode, DecodeDelegate<T>? decodeAuto, int length) : base(length)
    {
        this.encode = encode;
        this.encodeAuto = encodeAuto;
        this.decode = decode ?? ((ref ReadOnlySpan<byte> _) => ThrowHelper.ThrowNoSuitableConstructor<T>());
        this.decodeAuto = decodeAuto ?? ((ref ReadOnlySpan<byte> _) => ThrowHelper.ThrowNoSuitableConstructor<T>());
    }

    public override void Encode(ref Allocator allocator, T? item)
    {
        if (item is null)
            ThrowHelper.ThrowTupleNull<T>();
        this.encode.Invoke(ref allocator, item);
    }

    public override void EncodeAuto(ref Allocator allocator, T? item)
    {
        if (item is null)
            ThrowHelper.ThrowTupleNull<T>();
        this.encodeAuto.Invoke(ref allocator, item);
    }

    public override T Decode(in ReadOnlySpan<byte> span)
    {
        Debug.Assert(this.decode is not null);
        var body = span;
        return this.decode.Invoke(ref body);
    }

    public override T DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        return this.decodeAuto.Invoke(ref span);
    }
}
