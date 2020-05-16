using System;

namespace Mikodev.Binary.Creators.SpanLike
{
    internal sealed class SpanLikeConverter<T, E> : Converter<T>
    {
        private readonly SpanLikeAdapter<E> adapter;

        private readonly SpanLikeBuilder<T, E> builder;

        private readonly SpanLikeAbstractEncoder<T> encoder;

        public SpanLikeConverter(SpanLikeBuilder<T, E> builder, Converter<E> converter)
        {
            this.adapter = SpanLikeAdapterHelper.Create(converter);
            this.builder = builder;
            this.encoder = converter.Length > 0
                ? new SpanLikeConstantEncoder<T, E>(this.adapter, builder, converter.Length)
                : new SpanLikeVariableEncoder<T, E>(this.adapter, builder) as SpanLikeAbstractEncoder<T>;
        }

        public override void Encode(ref Allocator allocator, T item) => adapter.Encode(ref allocator, builder.Handle(item));

        public override T Decode(in ReadOnlySpan<byte> span) => builder.Invoke(span, adapter);

        public override void EncodeAuto(ref Allocator allocator, T item) => encoder.EncodeWithLengthPrefix(ref allocator, item);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => builder.Invoke(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span), adapter);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => encoder.EncodeWithLengthPrefix(ref allocator, item);

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => builder.Invoke(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span), adapter);
    }
}
