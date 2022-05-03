namespace Mikodev.Binary.Internal.Sequence.Builders;

using Mikodev.Binary.Internal.Sequence;
using System;

internal sealed class ReadOnlyMemoryBuilder<T> : SequenceBuilder<ReadOnlyMemory<T>, T>
{
    public override ReadOnlySpan<T> Handle(ReadOnlyMemory<T> item)
    {
        return item.Span;
    }

    public override ReadOnlyMemory<T> Invoke(ReadOnlySpan<byte> span, SequenceAdapter<T> invoke)
    {
        return invoke.Decode(span).GetArraySegment();
    }
}
