using System;

namespace Mikodev.Binary.Creators.Sequence
{
    internal abstract class SequenceBuilder<T, R>
    {
        public abstract T Invoke(ReadOnlySpan<byte> span, SequenceAdapter<T, R> adapter);
    }
}
