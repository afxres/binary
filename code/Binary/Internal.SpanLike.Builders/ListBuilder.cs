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
            Debug.Assert(this.ofList is not null);
#if NET5_0_OR_GREATER
            return System.Runtime.InteropServices.CollectionsMarshal.AsSpan(item);
#else
            if (item is { Count: var count } && count is not 0)
                return new ReadOnlySpan<T>(this.ofList.Invoke(item), 0, count);
            return default;
#endif
        }

        public override List<T> Invoke(ReadOnlySpan<byte> span, SpanLikeAdapter<T> adapter)
        {
            Debug.Assert(this.toList is not null);
            var (buffer, length) = adapter.Decode(span);
            Debug.Assert((uint)length <= (uint)buffer.Length);
            return this.toList.Invoke(buffer, length);
        }
    }
}
