using System;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.Generics
{
    internal sealed class GenericsConverter<T, R> : Converter<T>
    {
        private readonly GenericsAdapter<T, R> adapter;

        private readonly GenericsBuilder<T, R> builder;

        private readonly GenericsAbstractEncoder<T> encoder;

        public GenericsConverter(GenericsAdapter<T, R> adapter, GenericsBuilder<T, R> builder, GenericsCounter<T> counter, int itemLength)
        {
            this.adapter = adapter;
            this.builder = builder;
            this.encoder = itemLength > 0 && counter != null
                ? new GenericsConstantEncoder<T, R>(adapter, counter, itemLength)
                : new GenericsVariableEncoder<T, R>(adapter) as GenericsAbstractEncoder<T>;
            Debug.Assert(itemLength >= 0);
        }

        public override void Encode(ref Allocator allocator, T item) => adapter.Encode(ref allocator, item);

        public override T Decode(in ReadOnlySpan<byte> span) => builder.Invoke(span, adapter);

        public override void EncodeAuto(ref Allocator allocator, T item) => encoder.EncodeWithLengthPrefix(ref allocator, item);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => builder.Invoke(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span), adapter);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => encoder.EncodeWithLengthPrefix(ref allocator, item);

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => builder.Invoke(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span), adapter);
    }
}
