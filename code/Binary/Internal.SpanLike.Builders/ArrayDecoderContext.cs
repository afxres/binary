namespace Mikodev.Binary.Internal.SpanLike.Builders;

using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

internal sealed class ArrayDecoderContext<E> : SpanLikeDecoderContext<E[], E>
{
    public static ArrayDecoderContext<E> Instance = new ArrayDecoderContext<E>();

    public override Span<E> Invoke([NotNull] ref E[]? collection, int capacity)
    {
        Debug.Assert(capacity is not 0);
        Debug.Assert(collection is null);
        collection = new E[capacity];
        return new Span<E>(collection);
    }
}
