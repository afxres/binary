using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Adapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class MemoryConverter<T> : VariableConverter<Memory<T>>
    {
        private readonly Adapter<T> adapter;

        public MemoryConverter(Converter<T> converter) => adapter = AdapterHelper.Create(converter);

        public override void ToBytes(ref Allocator allocator, Memory<T> item) => adapter.Of(ref allocator, item.Span);

        public override Memory<T> ToValue(in ReadOnlySpan<byte> span) => adapter.To(in span);
    }
}
