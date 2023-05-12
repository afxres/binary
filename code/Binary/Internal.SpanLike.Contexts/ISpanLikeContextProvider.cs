namespace Mikodev.Binary.Internal.SpanLike.Contexts;

using System.Collections.Generic;

internal interface ISpanLikeContextProvider<E>
{
    SpanLikeForwardEncoder<E> GetEncoder();

    SpanLikeDecoder<E[]> GetDecoder();

    SpanLikeDecoder<List<E>> GetListDecoder();
}
