namespace Mikodev.Binary.Creators;

using System;

internal sealed class UriConverter : Converter<Uri?>
{
    private readonly Converter<string> converter;

    public UriConverter(Converter<string> converter) => this.converter = converter;

    private static Uri? DecodeInternal(string item) => string.IsNullOrEmpty(item) ? null : new Uri(item);

    private static string? EncodeInternal(Uri? item) => item?.OriginalString;

    public override Uri? Decode(in ReadOnlySpan<byte> span) => DecodeInternal(converter.Decode(in span));

    public override Uri? DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeInternal(converter.DecodeAuto(ref span));

    public override Uri? DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeInternal(converter.DecodeWithLengthPrefix(ref span));

    public override void Encode(ref Allocator allocator, Uri? item) => converter.Encode(ref allocator, EncodeInternal(item));

    public override void EncodeAuto(ref Allocator allocator, Uri? item) => converter.EncodeAuto(ref allocator, EncodeInternal(item));

    public override void EncodeWithLengthPrefix(ref Allocator allocator, Uri? item) => converter.EncodeWithLengthPrefix(ref allocator, EncodeInternal(item));
}
