namespace Mikodev.Binary.Internal.Contexts.Instance;

using Mikodev.Binary.Internal.Metadata;
using System;

internal sealed class TupleObjectDelegateConverter<T>(AllocatorAction<T> encode, AllocatorAction<T> encodeAuto, DecodeDelegate<T>? decode, DecodeDelegate<T>? decodeAuto, int length) : Converter<T>(length)
{
    private readonly AllocatorAction<T> encode = encode;

    private readonly AllocatorAction<T> encodeAuto = encodeAuto;

    private readonly DecodeDelegate<T>? decode = decode;

    private readonly DecodeDelegate<T>? decodeAuto = decodeAuto;

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
        var body = span;
        var decode = this.decode;
        if (decode is null)
            ThrowHelper.ThrowNoSuitableConstructor<T>();
        return decode.Invoke(ref body);
    }

    public override T DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        var decodeAuto = this.decodeAuto;
        if (decodeAuto is null)
            ThrowHelper.ThrowNoSuitableConstructor<T>();
        return decodeAuto.Invoke(ref span);
    }
}
