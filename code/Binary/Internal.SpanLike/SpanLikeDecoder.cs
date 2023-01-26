namespace Mikodev.Binary.Internal.SpanLike;

using System;

internal abstract class SpanLikeDecoder<T>
{
    public abstract T Invoke(ReadOnlySpan<byte> span);
}
