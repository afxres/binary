using Mikodev.Binary.Adapters.Abstractions;
using Mikodev.Binary.Delegates;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Adapters
{
    internal sealed class Adapter<T>
    {
        private readonly OfList<T> ofList;

        private readonly ToList<T> toList;

        private readonly AdapterMember<T> adapter;

        public Adapter(AdapterMember<T> adapter, OfList<T> ofList, ToList<T> toList)
        {
            Debug.Assert(adapter != null);
            Debug.Assert(ofList != null);
            Debug.Assert(toList != null);
            this.adapter = adapter;
            this.ofList = ofList;
            this.toList = toList;
        }

        public void Of(ref Allocator allocator, in ReadOnlySpan<T> span)
        {
            adapter.Of(ref allocator, in span);
        }

        public void OfList(ref Allocator allocator, List<T> list)
        {
            int length;
            if (list == null || (length = list.Count) == 0)
                return;
            var buffer = ofList == null ? list.ToArray() : ofList.Invoke(list);
            adapter.Of(ref allocator, new ReadOnlySpan<T>(buffer, 0, length));
        }

        public ArraySegment<T> To(in ReadOnlySpan<byte> span)
        {
            return adapter.To(in span);
        }

        public T[] ToArray(in ReadOnlySpan<byte> span)
        {
            var result = adapter.To(in span);
            Debug.Assert(result.Array.Length != 0 || ReferenceEquals(result.Array, Array.Empty<T>()));
            var buffer = result.Array;
            if (buffer.Length == result.Count)
                return buffer;
            return result.AsSpan().ToArray();
        }

        public List<T> ToList(in ReadOnlySpan<byte> span)
        {
            var result = adapter.To(in span);
            Debug.Assert(result.Array.Length != 0 || ReferenceEquals(result.Array, Array.Empty<T>()));
            if (toList == null)
                return new List<T>(result);
            return toList.Invoke(result.Array, result.Count);
        }
    }
}
