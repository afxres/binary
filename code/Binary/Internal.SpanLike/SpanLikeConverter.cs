namespace Mikodev.Binary.Internal.SpanLike;

using System;

internal sealed partial class SpanLikeConverter<T, E> : Converter<T>
{
    private readonly int itemLength;

    private readonly Converter<E> converter;

    private readonly SpanLikeEncoder<E> encoder;

    private readonly SpanLikeDecoder<E>? decoder;

    private readonly SpanLikeBuilder<T, E> builder;

    public SpanLikeConverter(SpanLikeBuilder<T, E> builder, Converter<E> converter)
    {
        this.encoder = SpanLikeContext.GetEncoder(converter);
        this.decoder = SpanLikeContext.GetDecoderOrDefault(converter);
        this.builder = builder;
        this.converter = converter;
        this.itemLength = converter.Length;
    }

    public override void Encode(ref Allocator allocator, T? item) => this.encoder.Encode(ref allocator, this.builder.Handle(item));

    public override void EncodeAuto(ref Allocator allocator, T? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override T Decode(in ReadOnlySpan<byte> span) => this.builder.Invoke(this.decoder, this.converter, span);

    public override T DecodeAuto(ref ReadOnlySpan<byte> span) => this.builder.Invoke(this.decoder, this.converter, Converter.DecodeWithLengthPrefix(ref span));

    public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => this.builder.Invoke(this.decoder, this.converter, Converter.DecodeWithLengthPrefix(ref span));
}
