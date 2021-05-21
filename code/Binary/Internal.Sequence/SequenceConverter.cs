using Mikodev.Binary.Internal.Metadata;
using System;

namespace Mikodev.Binary.Internal.Sequence
{
    internal sealed partial class SequenceConverter<T> : Converter<T>
    {
        private readonly DecodeReadOnlyDelegate<T> decoder;

        private readonly EncodeDelegate<T> encoder;

        public SequenceConverter(EncodeDelegate<T> encoder, DecodeReadOnlyDelegate<T> decoder)
        {
            this.encoder = encoder;
            this.decoder = decoder ?? ((in ReadOnlySpan<byte> _) => ThrowHelper.ThrowNoSuitableConstructor<T>());
        }

        public override void Encode(ref Allocator allocator, T item) => this.encoder.Invoke(ref allocator, item);

        public override void EncodeAuto(ref Allocator allocator, T item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override T Decode(in ReadOnlySpan<byte> span) => this.decoder.Invoke(span);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => this.decoder.Invoke(Converter.DecodeWithLengthPrefix(ref span));

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => this.decoder.Invoke(Converter.DecodeWithLengthPrefix(ref span));
    }
}
