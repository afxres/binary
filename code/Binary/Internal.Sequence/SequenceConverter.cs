using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Sequence
{
    internal sealed class SequenceConverter<T, R> : Converter<T>
    {
        private readonly SequenceAdapter<T, R> adapter;

        private readonly SequenceBuilder<T, R> builder;

        private readonly SequenceAbstractEncoder<T> encoder;

        public SequenceConverter(SequenceAdapter<T, R> adapter, SequenceBuilder<T, R> builder, SequenceCounter<T> counter, int itemLength)
        {
            this.adapter = adapter;
            this.builder = builder;
            this.encoder = itemLength > 0 && counter is not null
                ? new SequenceConstantEncoder<T, R>(adapter, counter, itemLength)
                : new SequenceVariableEncoder<T, R>(adapter) as SequenceAbstractEncoder<T>;
            Debug.Assert(itemLength >= 0);
        }

        public override void Encode(ref Allocator allocator, T item) => adapter.Encode(ref allocator, item);

        public override void EncodeAuto(ref Allocator allocator, T item) => encoder.EncodeWithLengthPrefix(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => encoder.EncodeWithLengthPrefix(ref allocator, item);

        public override T Decode(in ReadOnlySpan<byte> span) => builder.Invoke(span, adapter);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => builder.Invoke(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span), adapter);

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => builder.Invoke(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span), adapter);
    }
}
