﻿using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Adapters.Abstractions;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ListConverter<T> : VariableConverter<List<T>>
    {
        private readonly Adapter<T> adapter;

        public ListConverter(Adapter<T> adapter) => this.adapter = adapter;

        public override void ToBytes(ref Allocator allocator, List<T> item) => adapter.OfValue(ref allocator, item);

        public override List<T> ToValue(in ReadOnlySpan<byte> span) => adapter.ToValue(in span);
    }
}
