using Mikodev.Binary.CollectionAdapters;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ListFallbackBuilder<T> : CollectionBuilder<List<T>, ReadOnlyMemory<T>, ArraySegment<T>, T>
    {
        public override int Length(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(List<T> item) => item?.ToArray();

        public override List<T> To(CollectionAdapter<ArraySegment<T>, T> adapter, in ReadOnlySpan<byte> span) => new List<T>(adapter.To(in span));
    }
}
