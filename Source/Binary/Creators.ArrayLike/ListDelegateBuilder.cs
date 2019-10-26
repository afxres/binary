using Mikodev.Binary.CollectionModels;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ListDelegateBuilder<T> : ArrayLikeBuilder<List<T>, T>
    {
        private readonly OfList<T> ofList;

        private readonly ToList<T> toList;

        public ListDelegateBuilder(OfList<T> ofList, ToList<T> toList)
        {
            this.ofList = ofList;
            this.toList = toList;
        }

        public override int Count(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(List<T> item) => item is { Count: var count } && count > 0 ? new ReadOnlyMemory<T>(ofList.Invoke(item), 0, count) : default;

        public override List<T> To(CollectionAdapter<ArraySegment<T>> adapter, in ReadOnlySpan<byte> span)
        {
            var item = adapter.To(in span);
            Debug.Assert(item.Array != null && item.Offset == 0);
            return toList.Invoke(item.Array, item.Count);
        }
    }
}
