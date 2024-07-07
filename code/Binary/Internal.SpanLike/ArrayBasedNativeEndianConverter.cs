namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;

internal sealed class ArrayBasedNativeEndianConverter<T, E, A> : Converter<T> where A : struct, ISpanLikeAdapter<T, E>
{
    public override void Encode(ref Allocator allocator, T? item)
    {
        SpanLikeNativeEndianMethods.Encode(ref allocator, A.AsSpan(item));
    }

    public override void EncodeAuto(ref Allocator allocator, T? item)
    {
        EncodeWithLengthPrefix(ref allocator, item);
    }

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item)
    {
        SpanLikeNativeEndianMethods.EncodeWithLengthPrefix(ref allocator, A.AsSpan(item));
    }

    public override T Decode(in ReadOnlySpan<byte> span)
    {
        var values = SpanLikeNativeEndianMethods.GetArray<E>(span);
        var result = A.Invoke(values, values.Length);
        return result;
    }

    public override T DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        return DecodeWithLengthPrefix(ref span);
    }

    public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
    {
        var intent = Converter.DecodeWithLengthPrefix(ref span);
        var values = SpanLikeNativeEndianMethods.GetArray<E>(intent);
        var result = A.Invoke(values, values.Length);
        return result;
    }
}
