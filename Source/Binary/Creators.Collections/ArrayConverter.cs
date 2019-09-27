using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Adapters;
using System;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ArrayConverter<T> : VariableConverter<T[]>
    {
        private readonly Adapter<T> adapter;

        public ArrayConverter(Converter<T> converter) => adapter = AdapterHelper.Create(converter);

        public override void ToBytes(ref Allocator allocator, T[] item) => adapter.Of(ref allocator, item);

        public override T[] ToValue(in ReadOnlySpan<byte> span) => adapter.ToArray(span);
    }
}
