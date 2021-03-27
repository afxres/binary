using System;

namespace Mikodev.Binary.Internal.Sequence
{
    internal sealed partial class SequenceConverter<T> : Converter<T>
    {
        private readonly SequenceDecoder<T> decoder;

        private readonly SequenceEncoder<T> encoder;

        public SequenceConverter(SequenceEncoder<T> encoder, SequenceDecoder<T> decoder)
        {
            this.decoder = decoder;
            this.encoder = encoder;
        }

        public override void Encode(ref Allocator allocator, T item) => this.encoder.Encode(ref allocator, item);

        public override void EncodeAuto(ref Allocator allocator, T item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override T Decode(in ReadOnlySpan<byte> span) => this.decoder.Decode(span);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => this.decoder.Decode(Converter.DecodeWithLengthPrefix(ref span));

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => this.decoder.Decode(Converter.DecodeWithLengthPrefix(ref span));
    }
}
