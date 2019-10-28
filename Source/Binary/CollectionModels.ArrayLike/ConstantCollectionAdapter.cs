﻿using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;

namespace Mikodev.Binary.CollectionModels.ArrayLike
{
    internal sealed class ConstantCollectionAdapter<T> : ArrayLikeAdapter<T>
    {
        private readonly Converter<T> converter;

        public ConstantCollectionAdapter(Converter<T> converter) => this.converter = converter;

        public override void Of(ref Allocator allocator, ReadOnlyMemory<T> memory)
        {
            var span = memory.Span;
            for (var i = 0; i < span.Length; i++)
                converter.Encode(ref allocator, span[i]);
            Debug.Assert(converter.Length > 0);
        }

        public override MemoryItem<T> To(in ReadOnlySpan<byte> span)
        {
            Debug.Assert(converter.Length > 0);
            var byteCount = span.Length;
            if (byteCount == 0)
                return new MemoryItem<T>(Array.Empty<T>(), 0);
            var definition = converter.Length;
            var itemCount = CollectionHelper.GetItemCount(byteCount, definition);
            var items = new T[itemCount];
            for (var i = 0; i < itemCount; i++)
                items[i] = converter.Decode(span.Slice(i * definition));
            return new MemoryItem<T>(items, itemCount);
        }
    }
}
