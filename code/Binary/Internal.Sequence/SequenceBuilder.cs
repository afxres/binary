namespace Mikodev.Binary.Internal.Sequence;

using System;

internal abstract class SequenceBuilder<T, E>
{
    public abstract ReadOnlySpan<E> Handle(T? item);

    public abstract T Invoke(ReadOnlySpan<byte> span, SequenceAdapter<E> invoke);
}
