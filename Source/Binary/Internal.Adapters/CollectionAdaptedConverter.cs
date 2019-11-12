using System;
using System.Diagnostics;

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
            var data = builder.Of(item);
            var dataCount = item == null ? 0 : itemLength > 0 ? builder.Count(data) : CollectionBuilder.NoActualLength;
            if (dataCount != CollectionBuilder.NoActualLength)
            {
                var byteLength = checked(itemLength * dataCount);
                Debug.Assert(item == null || itemLength > 0);
                Debug.Assert(dataCount >= 0 && byteLength >= 0);
                PrimitiveHelper.EncodeNumber(ref allocator, byteLength);
                if (byteLength == 0)
                    return;
                adapter.Of(ref allocator, data);
            }
            else
            {
                var anchor = Allocator.AnchorLengthPrefix(ref allocator);
                adapter.Of(ref allocator, data);
                Allocator.AppendLengthPrefix(ref allocator, anchor);
            }
        }

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return builder.To(adapter, PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));
        }
    }
}
