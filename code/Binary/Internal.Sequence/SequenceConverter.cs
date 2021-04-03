using System;

namespace Mikodev.Binary.Internal.Sequence
{
    internal delegate T SequenceDecoder<out T>(ReadOnlySpan<byte> span);

    internal delegate void SequenceEncoder<in T>(ref Allocator allocator, T item);

    internal sealed partial class SequenceConverter<T> : Converter<T>
    {
        private readonly SequenceDecoder<T> decoder;

        private readonly SequenceEncoder<T> encoder;

        public SequenceConverter(SequenceEncoder<T> encoder, SequenceDecoder<T> decoder)
        {
            this.encoder = encoder;
            this.decoder = decoder ?? ThrowHelper.ThrowNoSuitableConstructor<T>;
        }

        public override void Encode(ref Allocator allocator, T item) => this.encoder.Invoke(ref allocator, item);

        public override void EncodeAuto(ref Allocator allocator, T item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override T Decode(in ReadOnlySpan<byte> span) => this.decoder.Invoke(span);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => this.decoder.Invoke(Converter.DecodeWithLengthPrefix(ref span));

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => this.decoder.Invoke(Converter.DecodeWithLengthPrefix(ref span));
    }
}
