using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.Builders
{
    internal sealed class ListBuilder<T> : ArrayLikeBuilder<List<T>, T>
    {
        private readonly OfList<T> ofList;

        private readonly ToList<T> toList;

        public ListBuilder(OfList<T> ofList, ToList<T> toList)
        {
            this.ofList = ofList;
            this.toList = toList;
        }

        public override ReadOnlyMemory<T> Of(List<T> item)
        {
            int itemCount;
            if (item is null || (itemCount = item.Count) == 0)
                return default;
            return new ReadOnlyMemory<T>(ofList.Invoke(item), 0, itemCount);
        }

        public override List<T> To(CollectionAdapter<MemoryItem<T>> adapter, ReadOnlySpan<byte> span)
        {
            var data = adapter.To(span);
            Debug.Assert(data.Buffer != null && data.Length >= 0 && data.Length <= data.Buffer.Length);
            return toList.Invoke(data.Buffer, data.Length);
        }
    }
}
