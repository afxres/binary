using Mikodev.Binary.CollectionAdapters;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ListDelegateBuilder<T> : CollectionBuilder<List<T>, ReadOnlyMemory<T>, T>
    {
        private readonly OfList<T> ofList;

        private readonly ToList<T> toList;

        public ListDelegateBuilder(OfList<T> ofList, ToList<T> toList)
        {
            this.ofList = ofList;
            this.toList = toList;
        }

        public override int Length(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(List<T> item) => item is { Count: var count } && count > 0 ? new ReadOnlyMemory<T>(ofList.Invoke(item), 0, count) : default;

        public override List<T> To(CollectionAdapter<T> adapter, in ReadOnlySpan<byte> span)
        {
            var item = adapter.To(in span);
            Debug.Assert(item.Array != null);
            return toList.Invoke(item.Array, item.Count);
        }
    }
}
