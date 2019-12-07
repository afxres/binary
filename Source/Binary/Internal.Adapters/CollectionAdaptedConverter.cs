using System;

namespace Mikodev.Binary.Internal.Adapters
{
    internal abstract class CollectionAdaptedConverter<T, U, R, E> : Converter<T>
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

        private void AppendWithLengthPrefix(ref Allocator allocator, T item)
        {
            int count;
            var value = builder.Of(item);
            if (itemLength > 0 && (count = builder.Count(value)) != CollectionBuilder.UnknownCount)
            {
                var byteLength = checked(itemLength * count);
                PrimitiveHelper.EncodeNumber(ref allocator, byteLength);
                adapter.Of(ref allocator, value);
            }
            else
            {
                var anchor = Allocator.Anchor(ref allocator, sizeof(int));
                adapter.Of(ref allocator, value);
                Allocator.AppendLengthPrefix(ref allocator, anchor, compact: true);
            }
        }

        private T DetachWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return builder.To(adapter, PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));
        }

        public override void Encode(ref Allocator allocator, T item) => adapter.Of(ref allocator, builder.Of(item));

        public override T Decode(in ReadOnlySpan<byte> span) => builder.To(adapter, span);

        public override void EncodeAuto(ref Allocator allocator, T item) => AppendWithLengthPrefix(ref allocator, item);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => DetachWithLengthPrefix(ref span);

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item) => AppendWithLengthPrefix(ref allocator, item);

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => DetachWithLengthPrefix(ref span);
    }
}
