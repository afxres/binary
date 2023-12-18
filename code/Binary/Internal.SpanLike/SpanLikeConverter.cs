namespace Mikodev.Binary.Internal.SpanLike;

using System;

internal sealed class SpanLikeConverter<T>(SpanLikeEncoder<T> encoder, SpanLikeDecoder<T> decoder) : Converter<T>
{
    private readonly SpanLikeEncoder<T> encoder = encoder;

    private readonly SpanLikeDecoder<T> decoder = decoder;

    public override void Encode(ref Allocator allocator, T? item) => this.encoder.Encode(ref allocator, item);

    public override void EncodeAuto(ref Allocator allocator, T? item) => this.encoder.EncodeWithLengthPrefix(ref allocator, item);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item) => this.encoder.EncodeWithLengthPrefix(ref allocator, item);

    public override T Decode(in ReadOnlySpan<byte> span) => this.decoder.Invoke(span);

    public override T DecodeAuto(ref ReadOnlySpan<byte> span) => this.decoder.Invoke(Converter.DecodeWithLengthPrefix(ref span));

    public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => this.decoder.Invoke(Converter.DecodeWithLengthPrefix(ref span));
}
