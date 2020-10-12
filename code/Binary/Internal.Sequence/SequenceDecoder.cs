using System;

namespace Mikodev.Binary.Internal.Sequence
{
    internal abstract class SequenceDecoder<T>
    {
        public abstract T Decode(ReadOnlySpan<byte> span);
    }
}
