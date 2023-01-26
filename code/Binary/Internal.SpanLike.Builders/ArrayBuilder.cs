namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;

internal readonly struct ArrayBuilder<E> : ISpanLikeBuilder<E[], E>
{
    public static E[] Invoke(E[] array, int count)
    {
        if (array.Length != count)
            Array.Resize(ref array, count);
        return array;
    }
}
