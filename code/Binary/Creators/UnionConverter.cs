namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal.Metadata;
using System;

internal sealed class UnionConverter<T>(AllocatorAction<T?> encode, AllocatorAction<T?> encodeAuto, DecodeDelegate<T?> decode, DecodeDelegate<T?> decodeAuto) : Converter<T?>()
{
    private readonly AllocatorAction<T?> encode = encode;

    private readonly AllocatorAction<T?> encodeAuto = encodeAuto;

    private readonly DecodeDelegate<T?> decode = decode;

    private readonly DecodeDelegate<T?> decodeAuto = decodeAuto;

    public override void Encode(ref Allocator allocator, T? item)
    {
        this.encode.Invoke(ref allocator, item);
    }

    public override void EncodeAuto(ref Allocator allocator, T? item)
    {
        this.encodeAuto.Invoke(ref allocator, item);
    }

    public override T? Decode(in ReadOnlySpan<byte> span)
    {
        var body = span;
        return this.decode.Invoke(ref body);
    }

    public override T? DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        return this.decodeAuto.Invoke(ref span);
    }
}
