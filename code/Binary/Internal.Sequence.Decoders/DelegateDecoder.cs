using System;

namespace Mikodev.Binary.Internal.Sequence.Decoders
{
    internal sealed class DelegateDecoder<T, R> : SequenceDecoder<T>
    {
        private readonly SequenceDecoder<R> decoder;

        private readonly Func<R, T> functor;

        public DelegateDecoder(SequenceDecoder<R> decoder, Func<R, T> functor)
        {
            this.decoder = decoder;
            this.functor = functor;
        }

        public override T Decode(ReadOnlySpan<byte> span)
        {
            var data = this.decoder.Decode(span);
            return this.functor.Invoke(data);
        }
    }
}
