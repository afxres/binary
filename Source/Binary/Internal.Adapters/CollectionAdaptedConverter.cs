using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class CollectionAdaptedConverter<T, U, R> : Converter<T>
    {
        private readonly int itemLength;

        private readonly CollectionAdapter<U, R> adapter;

        private readonly CollectionBuilder<T, U, R> builder;

        public CollectionAdaptedConverter(CollectionAdapter<U, R> adapter, CollectionBuilder<T, U, R> builder, int itemLength)
        {
            Debug.Assert(itemLength >= 0);
            this.adapter = adapter;
            this.builder = builder;
            this.itemLength = itemLength;
        }

        private void EncodeWithLengthPrefixInternal(ref Allocator allocator, T item)
        {
            int count;
            var value = builder.Of(item);
            if (itemLength > 0 && (count = value is null ? 0 : adapter.Count(value)) != -1)
            {
                var number = checked(itemLength * count);
                var numberLength = MemoryHelper.EncodeNumberLength((uint)number);
                MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
                adapter.Of(ref allocator, value);
            }
            else
            {
                var anchor = Allocator.Anchor(ref allocator, sizeof(int));
                adapter.Of(ref allocator, value);
                Allocator.AppendLengthPrefix(ref allocator, anchor, reduce: true);
            }
        }

        private T DecodeWithLengthPrefixInternal(ref ReadOnlySpan<byte> span)
        {
            return builder.To(adapter, PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));
        }

        public override void Encode(ref Allocator allocator, T item) => adapter.Of(ref allocator, builder.Of(item));

        public override T Decode(in ReadOnlySpan<byte> span) => builder.To(adapter, span);

        public override void EncodeAuto(ref Allocator allocator, T item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => DecodeWithLengthPrefixInternal(ref span);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => EncodeWithLengthPrefixInternal(ref allocator, item);

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DecodeWithLengthPrefixInternal(ref span);
    }
}
