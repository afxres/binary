namespace Mikodev.Binary.Internal.SpanLike;

using System;
using System.Diagnostics.CodeAnalysis;

internal abstract class SpanLikeDecoder<E>
{
    public abstract void Decode<T>(SpanLikeDecoderContext<T, E> context, [NotNull] ref T? collection, ReadOnlySpan<byte> span) where T : class;
}
