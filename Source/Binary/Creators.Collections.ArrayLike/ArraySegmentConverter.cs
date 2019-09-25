using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Adapters;
using System;

namespace Mikodev.Binary.Creators.Collections.ArrayLike
{
    internal sealed class ArraySegmentConverter<T> : VariableConverter<ArraySegment<T>>
    {
        private readonly Adapter<T> adapter;

        public ArraySegmentConverter(Adapter<T> adapter) => this.adapter = adapter;

        public override void ToBytes(ref Allocator allocator, ArraySegment<T> item) => adapter.Of(ref allocator, item);

        public override ArraySegment<T> ToValue(in ReadOnlySpan<byte> span) => adapter.To(in span);
    }
}
