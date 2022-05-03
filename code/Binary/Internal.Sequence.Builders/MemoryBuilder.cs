namespace Mikodev.Binary.Internal.Sequence.Builders;

using Mikodev.Binary.Internal.Sequence;
using System;

internal sealed class MemoryBuilder<T> : SequenceBuilder<Memory<T>, T>
{
    public override ReadOnlySpan<T> Handle(Memory<T> item)
    {
        return item.Span;
    }

    public override Memory<T> Invoke(ReadOnlySpan<byte> span, SequenceAdapter<T> invoke)
    {
        return invoke.Decode(span).GetArraySegment();
    }
}
