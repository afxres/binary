namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

internal readonly struct ImmutableArrayBuilder<E> : ISpanLikeBuilder<ImmutableArray<E>, E>
{
    public static ImmutableArray<E> Invoke(E[] array, int count)
    {
        if (array.Length != count)
            Array.Resize(ref array, count);
        return ImmutableCollectionsMarshal.AsImmutableArray(array);
    }
}
