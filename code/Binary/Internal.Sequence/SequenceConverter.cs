using System;

namespace Mikodev.Binary.Internal.Sequence
{
    internal sealed class SequenceConverter<T> : Converter<T>
    {
        private readonly SequenceDecoder<T> decoder;

        private readonly SequenceEncoder<T> encoder;

        private readonly SequenceAbstractEncoder<T> functor;

        public SequenceConverter(SequenceEncoder<T> encoder, SequenceDecoder<T> decoder, SequenceCounter<T> counter, int itemLength)
        {
            this.decoder = decoder;
            this.encoder = encoder;
            this.functor = itemLength > 0 && counter is not null
                ? new SequenceConstantEncoder<T>(encoder, counter, itemLength)
                : new SequenceVariableEncoder<T>(encoder) as SequenceAbstractEncoder<T>;
        }

        public override void Encode(ref Allocator allocator, T item) => this.encoder.Encode(ref allocator, item);

        public override void EncodeAuto(ref Allocator allocator, T item) => this.functor.EncodeWithLengthPrefix(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => this.functor.EncodeWithLengthPrefix(ref allocator, item);

        public override T Decode(in ReadOnlySpan<byte> span) => this.decoder.Decode(span);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => this.decoder.Decode(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => this.decoder.Decode(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));
    }
}
