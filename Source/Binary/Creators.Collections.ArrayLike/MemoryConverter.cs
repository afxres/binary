﻿using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Adapters;
using System;

namespace Mikodev.Binary.Creators.Collections.ArrayLike
{
    internal sealed class MemoryConverter<T> : VariableConverter<Memory<T>>
    {
        private readonly Adapter<T> adapter;

        public MemoryConverter(Adapter<T> adapter) => this.adapter = adapter;

        public override void ToBytes(ref Allocator allocator, Memory<T> item) => adapter.Of(ref allocator, item.Span);

        public override Memory<T> ToValue(in ReadOnlySpan<byte> span) => adapter.To(in span);
    }
}
