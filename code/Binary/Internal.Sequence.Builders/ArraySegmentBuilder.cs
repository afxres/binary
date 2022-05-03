namespace Mikodev.Binary.Internal.Sequence.Builders;

using Mikodev.Binary.Internal.Sequence;
using System;

internal sealed class ArraySegmentBuilder<T> : SequenceBuilder<ArraySegment<T>, T>
{
    public override ReadOnlySpan<T> Handle(ArraySegment<T> item)
    {
        return item;
    }

    public override ArraySegment<T> Invoke(ReadOnlySpan<byte> span, SequenceAdapter<T> invoke)
    {
        return invoke.Decode(span).GetArraySegment();
    }
}
