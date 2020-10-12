using System;

namespace Mikodev.Binary.Internal.Sequence.Decoders
{
    internal sealed class FallbackDecoder<T> : SequenceDecoder<T>
    {
        public override T Decode(ReadOnlySpan<byte> span) => ThrowHelper.ThrowNoSuitableConstructor<T>();
    }
}
