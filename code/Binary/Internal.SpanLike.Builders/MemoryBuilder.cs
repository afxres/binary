namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;

internal readonly struct MemoryBuilder<E> : ISpanLikeBuilder<Memory<E>, E>
{
    public static Memory<E> Invoke(E[] array, int count)
    {
        return new Memory<E>(array, 0, count);
    }
}
