namespace Mikodev.Binary.Internal.SpanLike;

using System;
using System.Diagnostics.CodeAnalysis;

internal abstract class SpanLikeDecoderContext<T, E>
{
    public abstract Span<E> Invoke([NotNull] ref T? collection, int capacity);
}
