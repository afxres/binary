﻿using Mikodev.Binary.CollectionModels;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ListFallbackBuilder<T> : ArrayLikeBuilder<List<T>, T>
    {
        public override int Count(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(List<T> item) => item?.ToArray();

        public override List<T> To(CollectionAdapter<ArraySegment<T>> adapter, in ReadOnlySpan<byte> span) => new List<T>(adapter.To(in span));
    }
}
