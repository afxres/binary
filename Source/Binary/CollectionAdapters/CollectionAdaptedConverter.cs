using Mikodev.Binary.Abstractions;
using System;

namespace Mikodev.Binary.CollectionAdapters
{
    internal sealed class CollectionAdaptedConverter<T, U, E> : VariableConverter<T>
    {
        private readonly int itemLength;

        private readonly CollectionAdapter<U, E> adapter;

        private readonly CollectionConvert<T, U, E> convert;

        public CollectionAdaptedConverter(Converter<E> converter, CollectionAdapter<U, E> adapter, CollectionConvert<T, U, E> convert)
        {
            itemLength = converter.Length;
            this.adapter = adapter;
            this.convert = convert;
        }

        public override void ToBytes(ref Allocator allocator, T item) => adapter.Of(ref allocator, convert.Of(item));

        public override T ToValue(in ReadOnlySpan<byte> span) => convert.To(adapter.To(in span));

        public override void ToBytesWithLengthPrefix(ref Allocator allocator, T item)
        {
            var data = convert.Of(item);
            int dataLength;
            var itemLength = this.itemLength;
            if (itemLength > 0 && (dataLength = convert.Length(data)) != CollectionConvert.NoActualLength)
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
            var data = PrimitiveHelper.DecodeWithLengthPrefix(ref span);
            var item = adapter.To(in data);
            return convert.To(in item);
        }
    }
}
