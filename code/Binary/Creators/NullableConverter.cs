namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
using System;

internal sealed class NullableConverter<T>(Converter<T> converter) : Converter<T?> where T : struct
{
    private const int None = 0;

    private const int Some = 1;

    private readonly Converter<T> converter = converter;

    public override void Encode(ref Allocator allocator, T? item)
    {
        var head = item.HasValue ? Some : None;
        Converter.Encode(ref allocator, head);
        if (head is None)
            return;
        this.converter.Encode(ref allocator, item.GetValueOrDefault());
    }

    public override void EncodeAuto(ref Allocator allocator, T? item)
    {
        var head = item.HasValue ? Some : None;
        Converter.Encode(ref allocator, head);
        if (head is None)
            return;
        this.converter.EncodeAuto(ref allocator, item.GetValueOrDefault());
    }

    public override T? Decode(in ReadOnlySpan<byte> span)
    {
        var body = span;
        var head = Converter.Decode(ref body);
        if (head is Some)
            return this.converter.Decode(in body);
        return head is None ? null : ThrowHelper.ThrowNullableTagInvalid<T>(head);
    }

    public override T? DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        var head = Converter.Decode(ref span);
        if (head is Some)
            return this.converter.DecodeAuto(ref span);
        return head is None ? null : ThrowHelper.ThrowNullableTagInvalid<T>(head);
    }
}
