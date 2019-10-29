﻿using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Adapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ReadOnlyMemoryBuilder<T> : ArrayLikeBuilder<ReadOnlyMemory<T>, T>
    {
        public override int Count(ReadOnlyMemory<T> item) => item.Length;

        public override ReadOnlyMemory<T> Of(ReadOnlyMemory<T> item) => item;

        public override ReadOnlyMemory<T> To(CollectionAdapter<MemoryItem<T>> adapter, in ReadOnlySpan<byte> span) => adapter.To(in span).AsArraySegment();
    }
}
