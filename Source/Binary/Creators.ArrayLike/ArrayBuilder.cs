using Mikodev.Binary.CollectionAdapters;
using System;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ArrayBuilder<T> : CollectionBuilder<T[], ReadOnlyMemory<T>, T>
    {
        public override int Length(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(T[] item) => item;

        public override T[] To(CollectionAdapter<T> adapter, in ReadOnlySpan<byte> span)
        {
            var item = adapter.To(in span);
            Debug.Assert(item.Array != null);
            var data = item.Array;
            if (data.Length == item.Count)
                return data;
            return item.AsSpan().ToArray();
        }
    }
}
