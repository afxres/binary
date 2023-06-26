namespace Mikodev.Binary.Experimental;

using System;

public sealed class LengthHeadedArrayConverter<E>(Converter<E> converter) : Converter<E[]?>
{
    /* Alternative Array Converter With Extra Header For Array Length
     * Layout: array length | array data
     */

    private readonly Converter<E> converter = converter;

    public override void Encode(ref Allocator allocator, E[]? item)
    {
        if (item is null)
            return;
        Converter.Encode(ref allocator, item.Length);
        var converter = this.converter;
        for (var i = 0; i < item.Length; i++)
            converter.EncodeAuto(ref allocator, item[i]);
        return;
    }

    public override E[]? Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return null;
        var body = span;
        var length = Converter.Decode(ref body);
        var result = new E[length];
        var converter = this.converter;
        for (var i = 0; i < result.Length; i++)
            result[i] = converter.DecodeAuto(ref body);
        return result;
    }
}
