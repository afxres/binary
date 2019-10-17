using Mikodev.Binary.CollectionAdapters;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ListDelegateCollectionConvert<T> : CollectionConvert<List<T>, ReadOnlyMemory<T>, T>
    {
        private readonly OfList<T> ofList;

        private readonly ToList<T> toList;

        public ListDelegateCollectionConvert(OfList<T> ofList, ToList<T> toList)
        {
            this.ofList = ofList;
            this.toList = toList;
        }

        public override int Length(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(List<T> item) => item is { Count: var count } && count > 0 ? new ReadOnlyMemory<T>(ofList.Invoke(item), 0, count) : default;

        public override List<T> To(in ArraySegment<T> item) => toList.Invoke(item.Array, item.Count);
    }
}
