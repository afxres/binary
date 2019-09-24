using Mikodev.Binary.Adapters.Abstractions;
using Mikodev.Binary.Delegates;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Adapters
{
    internal sealed class Adapter<T>
    {
        private readonly GetListItems<T> get;

        private readonly SetListItems<T> set;

        private readonly AdapterMember<T> adapter;

        public Adapter(AdapterMember<T> adapter, GetListItems<T> get, SetListItems<T> set)
        {
            this.adapter = adapter;
            this.get = get;
            this.set = set;
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
            var buffer = get == null ? list.ToArray() : get.Invoke(list);
            adapter.Of(ref allocator, new ReadOnlySpan<T>(buffer, 0, length));
        }

        public T[] ToArray(in ReadOnlySpan<byte> span)
        {
            adapter.To(in span, out var result, out var length);
            Debug.Assert(length <= result.Length);
            if (result.Length == length)
                return result;
            return new ReadOnlySpan<T>(result, 0, length).ToArray();
        }

        public List<T> ToList(in ReadOnlySpan<byte> span)
        {
            adapter.To(in span, out var result, out var length);
            Debug.Assert(length <= result.Length);
            if (set == null)
                return new List<T>(new ArraySegment<T>(result, 0, length));
            var list = new List<T>();
            set.Invoke(list, result, length);
            return list;
        }
    }
}
