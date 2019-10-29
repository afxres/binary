using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
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

        public override List<T> To(CollectionAdapter<MemoryItem<T>> adapter, in ReadOnlySpan<byte> span)
        {
            var data = adapter.To(in span);
            Debug.Assert(data.Buffer != null && data.Length >= 0 && data.Length <= data.Buffer.Length);
            return toList.Invoke(data.Buffer, data.Length);
        }
    }
}
