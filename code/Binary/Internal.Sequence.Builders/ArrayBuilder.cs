namespace Mikodev.Binary.Internal.Sequence.Builders;

using Mikodev.Binary.Internal.Sequence;
using System;

internal sealed class ArrayBuilder<T> : SequenceBuilder<T[], T>
{
    public override ReadOnlySpan<T> Handle(T[]? item)
    {
        return item;
    }

    public override T[] Invoke(ReadOnlySpan<byte> span, SequenceAdapter<T> invoke)
    {
        return invoke.Decode(span).GetArray();
    }
}
