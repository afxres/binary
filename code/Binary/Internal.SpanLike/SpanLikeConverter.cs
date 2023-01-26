namespace Mikodev.Binary.Internal.SpanLike;

using System;

internal sealed partial class SpanLikeConverter<T, E> : Converter<T>
{
    private readonly int itemLength;

    private readonly SpanLikeEncoder<E> encoder;

    private readonly SpanLikeDecoder<T> decoder;

    private readonly SpanLikeAdapter<T, E> adapter;

    public SpanLikeConverter(SpanLikeEncoder<E> encoder, SpanLikeDecoder<T> decoder, SpanLikeAdapter<T, E> adapter, Converter<E> converter)
    {
        this.adapter = adapter;
        this.decoder = decoder;
        this.encoder = encoder;
        this.itemLength = converter.Length;
    }

    public override void Encode(ref Allocator allocator, T? item) => this.encoder.Encode(ref allocator, this.adapter.Invoke(item));

    public override void EncodeAuto(ref Allocator allocator, T? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override T Decode(in ReadOnlySpan<byte> span) => this.decoder.Invoke(span);

    public override T DecodeAuto(ref ReadOnlySpan<byte> span) => this.decoder.Invoke(Converter.DecodeWithLengthPrefix(ref span));

    public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => this.decoder.Invoke(Converter.DecodeWithLengthPrefix(ref span));
}
