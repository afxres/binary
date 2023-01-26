namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using System;

internal sealed class ArrayAdapter<E> : SpanLikeAdapter<E[], E>
{
    public override ReadOnlySpan<E> Invoke(E[]? item)
    {
        return item;
    }
}
