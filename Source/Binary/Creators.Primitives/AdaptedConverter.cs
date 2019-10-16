using System;
using System.Diagnostics;

namespace Mikodev.Binary.Creators.Primitives
{
    internal sealed class AdaptedConverter<T, E> : Converter<T>
    {
        private readonly Adapter<T, E> adapter;

        private readonly Converter<E> converter;

        public AdaptedConverter(Adapter<T, E> adapter, Converter<E> converter) : base(converter.Length)
        {
            Debug.Assert(adapter != null);
            Debug.Assert(converter != null);
            this.adapter = adapter;
            this.converter = converter;
        }

        public override void ToBytes(ref Allocator allocator, T item) => converter.ToBytes(ref allocator, adapter.OfValue(item));

        public override T ToValue(in ReadOnlySpan<byte> span) => adapter.ToValue(converter.ToValue(in span));

        public override void ToBytesWithMark(ref Allocator allocator, T item) => converter.ToBytesWithMark(ref allocator, adapter.OfValue(item));

        public override T ToValueWithMark(ref ReadOnlySpan<byte> span) => adapter.ToValue(converter.ToValueWithMark(ref span));

        public override void ToBytesWithLengthPrefix(ref Allocator allocator, T item) => converter.ToBytesWithLengthPrefix(ref allocator, adapter.OfValue(item));

        public override T ToValueWithLengthPrefix(ref ReadOnlySpan<byte> span) => adapter.ToValue(converter.ToValueWithLengthPrefix(ref span));
    }
}
