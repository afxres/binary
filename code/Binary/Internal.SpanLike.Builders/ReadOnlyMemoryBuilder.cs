namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;

internal readonly struct ReadOnlyMemoryBuilder<E> : ISpanLikeBuilder<ReadOnlyMemory<E>, E>
{
    public static ReadOnlyMemory<E> Invoke(E[] array, int count)
    {
        return new ReadOnlyMemory<E>(array, 0, count);
    }
}
