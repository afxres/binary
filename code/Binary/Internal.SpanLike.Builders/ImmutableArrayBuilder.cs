namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

internal readonly struct ImmutableArrayBuilder<E> : ISpanLikeBuilder<ImmutableArray<E>, E>
{
    public static ImmutableArray<E> Invoke(E[] array, int count)
    {
        // TODO: use 'ImmutableCollectionsMarshal.AsImmutableArray'
        static ImmutableArray<E> Create(E[] array) => Unsafe.As<E[], ImmutableArray<E>>(ref array);

        if (array.Length != count)
            Array.Resize(ref array, count);
        return Create(array);
    }
}
