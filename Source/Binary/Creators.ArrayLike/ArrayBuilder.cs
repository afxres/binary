﻿using Mikodev.Binary.CollectionModels;
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
            var item = adapter.To(in span);
            Debug.Assert(item.Array != null);
            var data = item.Array;
            if (data.Length == item.Count)
                return data;
            return item.AsSpan().ToArray();
        }
    }
}