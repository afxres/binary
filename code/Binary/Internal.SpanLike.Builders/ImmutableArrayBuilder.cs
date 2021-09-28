namespace Mikodev.Binary.Internal.SpanLike.Builders;

using System;
using System.Collections.Immutable;

internal sealed class ImmutableArrayBuilder<T> : SpanLikeBuilder<ImmutableArray<T>, T>
{
    public override ReadOnlySpan<T> Handle(ImmutableArray<T> item)
    {
        return NativeModule.AsSpan(item);
    }

    public override ImmutableArray<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> invoke)
    {
        return NativeModule.CreateImmutableArray(invoke.Decode(span).GetArray());
    }
}
