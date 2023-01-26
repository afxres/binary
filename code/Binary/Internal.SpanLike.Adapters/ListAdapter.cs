namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

internal sealed class ListAdapter<E> : SpanLikeAdapter<List<E>, E>
{
    public override ReadOnlySpan<E> Invoke(List<E>? item)
    {
        return CollectionsMarshal.AsSpan(item);
    }
}
