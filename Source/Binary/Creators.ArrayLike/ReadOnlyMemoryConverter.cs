using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Adapters;
using System;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ReadOnlyMemoryConverter<T> : VariableConverter<ReadOnlyMemory<T>>
    {
        private readonly Adapter<T> adapter;

        public ReadOnlyMemoryConverter(Converter<T> converter) => adapter = AdapterHelper.Create(converter);

        public override void ToBytes(ref Allocator allocator, ReadOnlyMemory<T> item) => adapter.Of(ref allocator, item.Span);

        public override ReadOnlyMemory<T> ToValue(in ReadOnlySpan<byte> span) => adapter.To(in span);
    }
}
