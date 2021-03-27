using System;

namespace Mikodev.Binary.Internal.SpanLike
{
    internal sealed partial class SpanLikeConverter<T, E> : Converter<T>
    {
        private readonly int itemLength;

        private readonly SpanLikeAdapter<E> adapter;

        private readonly SpanLikeBuilder<T, E> builder;

        public SpanLikeConverter(SpanLikeBuilder<T, E> builder, Converter<E> converter)
        {
            this.adapter = SpanLikeAdapterHelper.Create(converter);
            this.builder = builder;
            this.itemLength = converter.Length;
        }

        public override void Encode(ref Allocator allocator, T item) => this.adapter.Encode(ref allocator, this.builder.Handle(item));

        public override void EncodeAuto(ref Allocator allocator, T item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override T Decode(in ReadOnlySpan<byte> span) => this.builder.Invoke(span, this.adapter);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => this.builder.Invoke(Converter.DecodeWithLengthPrefix(ref span), this.adapter);

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => this.builder.Invoke(Converter.DecodeWithLengthPrefix(ref span), this.adapter);
    }
}
