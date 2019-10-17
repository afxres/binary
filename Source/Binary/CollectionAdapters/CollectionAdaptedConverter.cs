using System;

namespace Mikodev.Binary.CollectionAdapters
{
    internal sealed class CollectionAdaptedConverter<T, E> : Converter<T>
    {
        private readonly int itemLength;

        private readonly CollectionAdapter<E> adapter;

        private readonly CollectionConvert<T, E> convert;

        public CollectionAdaptedConverter(Converter<E> converter, CollectionConvert<T, E> convert) : base(0)
        {
            itemLength = converter.Length;
            adapter = (CollectionAdapter<E>)CollectionAdapterHelper.Create(converter);
            this.convert = convert;
        }

        public override void ToBytes(ref Allocator allocator, T item) => adapter.Of(ref allocator, convert.Of(item));

        public override T ToValue(in ReadOnlySpan<byte> span) => convert.To(adapter.To(in span));

        public override void ToBytesWithMark(ref Allocator allocator, T item) => ToBytesWithLengthPrefix(ref allocator, item);

        public override T ToValueWithMark(ref ReadOnlySpan<byte> span) => ToValueWithLengthPrefix(ref span);

        public override void ToBytesWithLengthPrefix(ref Allocator allocator, T item)
        {
            var span = convert.Of(item);
            var itemLength = this.itemLength;
            if (itemLength > 0)
            {
                var byteLength = checked(itemLength * span.Length);
                PrimitiveHelper.EncodeLengthPrefix(ref allocator, (uint)byteLength);
                adapter.Of(ref allocator, in span);
            }
            else
            {
                allocator.LengthPrefixAnchor(out var anchor);
                adapter.Of(ref allocator, in span);
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
