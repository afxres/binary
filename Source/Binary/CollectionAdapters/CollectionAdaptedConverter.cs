using Mikodev.Binary.Abstractions;
using System;

namespace Mikodev.Binary.CollectionAdapters
{
    internal abstract class CollectionAdaptedConverter<T, U, E> : VariableConverter<T>
    {
        private readonly int itemLength;

        private readonly CollectionAdapter<U, E> adapter;

        private readonly CollectionBuilder<T, U, E> builder;

        public CollectionAdaptedConverter(Converter<E> converter, CollectionAdapter<U, E> adapter, CollectionBuilder<T, U, E> builder)
        {
            itemLength = converter.Length;
            this.adapter = adapter;
            this.builder = builder;
        }

        public override void ToBytes(ref Allocator allocator, T item) => adapter.Of(ref allocator, builder.Of(item));

        public override T ToValue(in ReadOnlySpan<byte> span) => builder.To(adapter, in span);

        public override void ToBytesWithLengthPrefix(ref Allocator allocator, T item)
        {
            int dataLength;
            var data = builder.Of(item);
            if (data == null)
            {
                PrimitiveHelper.EncodeLengthPrefix(ref allocator, 0);
            }
            else if (itemLength > 0 && (dataLength = builder.Length(data)) != CollectionBuilder.NoActualLength)
            {
                var byteLength = checked(itemLength * dataLength);
                PrimitiveHelper.EncodeLengthPrefix(ref allocator, (uint)byteLength);
                adapter.Of(ref allocator, data);
            }
            else
            {
                allocator.LengthPrefixAnchor(out var anchor);
                adapter.Of(ref allocator, data);
                allocator.LengthPrefixFinish(anchor);
            }
        }

        public override T ToValueWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return builder.To(adapter, PrimitiveHelper.DecodeWithLengthPrefix(ref span));
        }
    }
}
