using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class ArrayLikeConstantAdapter<T> : ArrayLikeAdapter<T>
    {
        private readonly Converter<T> converter;

        public ArrayLikeConstantAdapter(Converter<T> converter) => this.converter = converter;

        public override void Of(ref Allocator allocator, ReadOnlyMemory<T> memory)
        {
            var span = memory.Span;
            for (var i = 0; i < span.Length; i++)
                converter.Encode(ref allocator, span[i]);
            Debug.Assert(converter.Length > 0);
        }

        public override MemoryItem<T> To(ReadOnlySpan<byte> span)
        {
            Debug.Assert(converter.Length > 0);
            var byteLength = span.Length;
            if (byteLength == 0)
                return new MemoryItem<T>(Array.Empty<T>(), 0);
            var definition = converter.Length;
            var itemCount = CollectionAdapterHelper.GetItemCount(byteLength, definition, typeof(T));
            var items = new T[itemCount];
            for (var i = 0; i < itemCount; i++)
                items[i] = converter.Decode(span.Slice(i * definition));
            return new MemoryItem<T>(items, itemCount);
        }
    }
}
