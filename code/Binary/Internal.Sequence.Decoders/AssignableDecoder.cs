using System;

namespace Mikodev.Binary.Internal.Sequence.Decoders
{
    internal sealed class AssignableDecoder<T, R> : SequenceDecoder<T>
    {
        private readonly SequenceDecoder<R> decoder;

        public AssignableDecoder(SequenceDecoder<R> decoder) => this.decoder = decoder;

        public override T Decode(ReadOnlySpan<byte> span) => (T)(object)this.decoder.Decode(span);
    }
}
