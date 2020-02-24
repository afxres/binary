﻿using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class ArrayLikeConstantAdapter<T> : ArrayLikeAdapter<T>
    {
        private readonly Converter<T> converter;

        public ArrayLikeConstantAdapter(Converter<T> converter) => this.converter = converter;

        public override void Of(ref Allocator allocator, ReadOnlyMemory<T> memory)
        {
            foreach (var i in memory.Span)
                converter.Encode(ref allocator, i);
            Debug.Assert(converter.Length > 0);
        }

        public override ArraySegment<T> To(ReadOnlySpan<byte> span)
        {
            Debug.Assert(converter.Length > 0);
            var byteLength = span.Length;
            if (byteLength == 0)
                return new ArraySegment<T>(Array.Empty<T>());
            var definition = converter.Length;
            var itemCount = CollectionAdapterHelper.GetItemCount(byteLength, definition, typeof(T));
            var items = new T[itemCount];
            for (var i = 0; i < itemCount; i++)
                items[i] = converter.Decode(span.Slice(i * definition));
            return new ArraySegment<T>(items, 0, itemCount);
        }
    }
}
