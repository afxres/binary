using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Adapters.Abstractions;
using System;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ArrayConverter<T> : VariableConverter<T[]>
    {
        private readonly Adapter<T> adapter;

        public ArrayConverter(Adapter<T> adapter) => this.adapter = adapter;

        public override void ToBytes(ref Allocator allocator, T[] item) => adapter.OfArray(ref allocator, item);

        public override T[] ToValue(in ReadOnlySpan<byte> span) => adapter.ToArray(span);
    }
}
