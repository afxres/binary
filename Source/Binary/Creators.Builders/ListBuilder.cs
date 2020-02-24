using Mikodev.Binary.Internal.Adapters;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.Builders
{
    internal sealed class ListBuilder<T> : ArrayLikeBuilder<List<T>, T>
    {
        private readonly Func<List<T>, T[]> ofList;

        private readonly Func<T[], int, List<T>> toList;

        public ListBuilder(Func<List<T>, T[]> ofList, Func<T[], int, List<T>> toList)
        {
            this.ofList = ofList;
            this.toList = toList;
        }

        public override ReadOnlyMemory<T> Of(List<T> item)
        {
            if (item is { Count: var count } && count != 0)
                return new ReadOnlyMemory<T>(ofList.Invoke(item), 0, count);
            return default;
        }

        public override List<T> To(CollectionAdapter<ArraySegment<T>> adapter, ReadOnlySpan<byte> span)
        {
            var data = adapter.To(span);
            Debug.Assert(data.Array != null && data.Offset == 0);
            return toList.Invoke(data.Array, data.Count);
        }
    }
}
