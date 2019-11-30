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

        public override void Encode(ref Allocator allocator, T item)
        {
            adapter.Of(ref allocator, builder.Of(item));
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            return builder.To(adapter, span);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var value = item == null ? default : builder.Of(item);
            var count = item == null ? default : itemLength > 0 ? builder.Count(value) : CollectionBuilder.UnknownCount;
            if (count != CollectionBuilder.UnknownCount)
            {
                var byteLength = checked(itemLength * count);
                PrimitiveHelper.EncodeNumber(ref allocator, byteLength);
                if (byteLength == 0)
                    return;
                adapter.Of(ref allocator, value);
            }
            else
            {
                var anchor = Allocator.Anchor(ref allocator, sizeof(int));
                adapter.Of(ref allocator, value);
                Allocator.AppendLengthPrefix(ref allocator, anchor, compact: true);
            }
        }

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return builder.To(adapter, PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));
        }
    }
}
