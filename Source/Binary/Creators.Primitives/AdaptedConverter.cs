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

        public override void Encode(ref Allocator allocator, T item) => converter.Encode(ref allocator, adapter.Of(item));

        public override T Decode(in ReadOnlySpan<byte> span) => adapter.To(converter.Decode(in span));

        public override void EncodeAuto(ref Allocator allocator, T item) => converter.EncodeAuto(ref allocator, adapter.Of(item));

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => adapter.To(converter.DecodeAuto(ref span));

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => converter.EncodeWithLengthPrefix(ref allocator, adapter.Of(item));

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => adapter.To(converter.DecodeWithLengthPrefix(ref span));
    }
}
