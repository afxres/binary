using Mikodev.Binary.CollectionModels;
using System;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArrayBuilder<T> : ArrayLikeBuilder<T[], T>
    {
        public override int Count(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(T[] item) => item;

        public override T[] To(CollectionAdapter<ArraySegment<T>> adapter, in ReadOnlySpan<byte> span)
        {
            var data = adapter.To(in span);
            Debug.Assert(data.Array != null && data.Offset == 0);
            var item = data.Array;
            if (item.Length == data.Count)
                return item;
            return data.AsSpan().ToArray();
        }
    }
}
