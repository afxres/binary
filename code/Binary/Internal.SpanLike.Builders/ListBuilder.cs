using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Builders
{
    internal sealed class ListBuilder<T> : SpanLikeBuilder<List<T>, T>
    {
        private readonly Func<List<T>, T[]> ofList;

        private readonly Func<T[], int, List<T>> toList;

        public ListBuilder(Func<List<T>, T[]> ofList, Func<T[], int, List<T>> toList)
        {
            this.ofList = ofList;
            this.toList = toList;
        }

        public override ReadOnlySpan<T> Handle(List<T> item)
        {
            if (item is { Count: var count } && count is not 0)
                return new ReadOnlySpan<T>(ofList.Invoke(item), 0, count);
            return default;
        }

        public override List<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            var data = adapter.Decode(span);
            Debug.Assert((uint)data.Length <= (uint)data.Memory.Length);
            return toList.Invoke(data.Memory, data.Length);
        }
    }
}
