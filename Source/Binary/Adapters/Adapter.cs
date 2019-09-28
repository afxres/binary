﻿using Mikodev.Binary.Adapters.Abstractions;
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

        private readonly AdapterMember<T> member;

        public Adapter(AdapterMember<T> member, OfList<T> ofList, ToList<T> toList)
        {
            Debug.Assert(member != null);
            Debug.Assert(ofList != null);
            Debug.Assert(toList != null);
            this.member = member;
            this.ofList = ofList;
            this.toList = toList;
        }

        public void Of(ref Allocator allocator, in ReadOnlySpan<T> span)
        {
            member.Of(ref allocator, in span);
        }

        public void OfList(ref Allocator allocator, List<T> list)
        {
            int itemCount;
            if (list == null || (itemCount = list.Count) == 0)
                return;
            var items = ofList == null ? list.ToArray() : ofList.Invoke(list);
            member.Of(ref allocator, new ReadOnlySpan<T>(items, 0, itemCount));
        }

        public ArraySegment<T> To(in ReadOnlySpan<byte> span)
        {
            return member.To(in span);
        }

        public T[] ToArray(in ReadOnlySpan<byte> span)
        {
            var result = member.To(in span);
            Debug.Assert(result.Array.Length != 0 || ReferenceEquals(result.Array, Array.Empty<T>()));
            var buffer = result.Array;
            if (buffer.Length == result.Count)
                return buffer;
            return result.AsSpan().ToArray();
        }

        public List<T> ToList(in ReadOnlySpan<byte> span)
        {
            var result = member.To(in span);
            Debug.Assert(result.Array.Length != 0 || ReferenceEquals(result.Array, Array.Empty<T>()));
            if (toList == null)
                return new List<T>(result);
            return toList.Invoke(result.Array, result.Count);
        }
    }
}
