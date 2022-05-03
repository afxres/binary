namespace Mikodev.Binary.Internal.Sequence.Builders;

using Mikodev.Binary.Internal.Sequence;
using System;
using System.Collections.Immutable;

internal sealed class ImmutableArrayBuilder<T> : SequenceBuilder<ImmutableArray<T>, T>
{
    public override ReadOnlySpan<T> Handle(ImmutableArray<T> item)
    {
        return NativeModule.AsSpan(item);
    }

    public override ImmutableArray<T> Invoke(ReadOnlySpan<byte> span, SequenceAdapter<T> invoke)
    {
        return NativeModule.CreateImmutableArray(invoke.Decode(span).GetArray());
    }
}
