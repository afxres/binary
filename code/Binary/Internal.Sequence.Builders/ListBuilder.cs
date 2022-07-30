namespace Mikodev.Binary.Internal.Sequence.Builders;

using Mikodev.Binary.Internal.Sequence;
using System;
using System.Collections.Generic;

internal sealed class ListBuilder<T> : SequenceBuilder<List<T>, T>
{
    public override ReadOnlySpan<T> Handle(List<T>? item)
    {
        return NativeModule.AsReadOnlySpan(item);
    }

    public override List<T> Invoke(ReadOnlySpan<byte> span, SequenceAdapter<T> invoke)
    {
        return invoke.Decode(span).GetList();
    }
}
