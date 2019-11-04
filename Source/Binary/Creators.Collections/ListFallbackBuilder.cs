﻿using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ListFallbackBuilder<T> : ArrayLikeBuilder<List<T>, T>
    {
        public override int Count(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(List<T> item) => item?.ToArray();

        public override List<T> To(CollectionAdapter<MemoryItem<T>> adapter, ReadOnlySpan<byte> span) => new List<T>(adapter.To(span).AsArraySegment());
    }
}
