namespace Mikodev.Binary.Internal.SpanLike;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

internal sealed class ListNativeEndianConverter<E> : Converter<List<E>>
{
    public override void Encode(ref Allocator allocator, List<E>? item)
    {
        SpanLikeNativeEndianMethods.Encode<E>(ref allocator, CollectionsMarshal.AsSpan(item));
    }

    public override void EncodeAuto(ref Allocator allocator, List<E>? item)
    {
        EncodeWithLengthPrefix(ref allocator, item);
    }

    public override void EncodeWithLengthPrefix(ref Allocator allocator, List<E>? item)
    {
        SpanLikeNativeEndianMethods.EncodeWithLengthPrefix<E>(ref allocator, CollectionsMarshal.AsSpan(item));
    }

    public override List<E> Decode(in ReadOnlySpan<byte> span)
    {
        return SpanLikeNativeEndianMethods.GetList<E>(span);
    }

    public override List<E> DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        return DecodeWithLengthPrefix(ref span);
    }

    public override List<E> DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
    {
        return SpanLikeNativeEndianMethods.GetList<E>(Converter.DecodeWithLengthPrefix(ref span));
    }
}
