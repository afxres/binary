﻿using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike
{
    internal sealed class SpanLikeConstantEncoder<T, E> : SpanLikeAbstractEncoder<T>
    {
        private readonly int itemLength;

        private readonly SpanLikeAdapter<E> adapter;

        private readonly SpanLikeBuilder<T, E> builder;

        public SpanLikeConstantEncoder(SpanLikeAdapter<E> adapter, SpanLikeBuilder<T, E> builder, int itemLength)
        {
            this.adapter = adapter;
            this.builder = builder;
            this.itemLength = itemLength;
            Debug.Assert(itemLength > 0);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var data = builder.Handle(item);
            var number = checked(itemLength * data.Length);
            var numberLength = MemoryHelper.EncodeNumberLength((uint)number);
            MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
            adapter.Encode(ref allocator, data);
        }
    }
}