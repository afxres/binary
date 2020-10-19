using System;

namespace Mikodev.Binary.Internal.SpanLike
{
    internal sealed class SpanLikeConverter<T, E> : Converter<T>
    {
        private readonly SpanLikeAdapter<E> adapter;

        private readonly SpanLikeBuilder<T, E> builder;

        private readonly SpanLikeAbstractEncoder<T> functor;

        public SpanLikeConverter(SpanLikeBuilder<T, E> builder, Converter<E> converter)
        {
            this.adapter = SpanLikeAdapterHelper.Create(converter);
            this.builder = builder;
            this.functor = converter.Length > 0
                ? new SpanLikeConstantEncoder<T, E>(this.adapter, builder, converter.Length)
                : new SpanLikeVariableEncoder<T, E>(this.adapter, builder);
        }

        public override void Encode(ref Allocator allocator, T item) => this.adapter.Encode(ref allocator, this.builder.Handle(item));

        public override void EncodeAuto(ref Allocator allocator, T item) => this.functor.EncodeWithLengthPrefix(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => this.functor.EncodeWithLengthPrefix(ref allocator, item);

        public override T Decode(in ReadOnlySpan<byte> span) => this.builder.Invoke(span, this.adapter);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => this.builder.Invoke(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span), this.adapter);

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => this.builder.Invoke(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span), this.adapter);
    }
}
