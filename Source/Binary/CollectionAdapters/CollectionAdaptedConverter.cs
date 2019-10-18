﻿using Mikodev.Binary.Abstractions;
using System;

namespace Mikodev.Binary.CollectionAdapters
{
    internal abstract class CollectionAdaptedConverter<T, U, R, E> : VariableConverter<T>
    {
        private readonly int itemLength;

        private readonly CollectionAdapter<U, R, E> adapter;

        private readonly CollectionBuilder<T, U, R, E> builder;

        public CollectionAdaptedConverter(Converter<E> converter, CollectionAdapter<U, R, E> adapter, CollectionBuilder<T, U, R, E> builder)
        {
            itemLength = converter.Length;
            this.adapter = adapter;
            this.builder = builder;
        }

        public override void Encode(ref Allocator allocator, T item) => adapter.Of(ref allocator, builder.Of(item));

        public override T Decode(in ReadOnlySpan<byte> span) => builder.To(adapter, in span);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            int dataLength;
            var data = builder.Of(item);
            if (data == null || (dataLength = builder.Length(data)) == 0)
            {
                PrimitiveHelper.EncodeNumber(ref allocator, 0);
            }
            else if (itemLength > 0 && dataLength != CollectionBuilder.NoActualLength)
            {
                var byteLength = checked(itemLength * dataLength);
                PrimitiveHelper.EncodeNumber(ref allocator, byteLength);
                adapter.Of(ref allocator, data);
            }
            else
            {
                allocator.LengthPrefixAnchor(out var anchor);
                adapter.Of(ref allocator, data);
                allocator.LengthPrefixFinish(anchor);
            }
        }

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return builder.To(adapter, PrimitiveHelper.DecodeWithLengthPrefix(ref span));
        }
    }
}
