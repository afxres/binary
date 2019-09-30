using Mikodev.Binary.Adapters.Abstractions;
using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;

namespace Mikodev.Binary.Adapters.Implementations
{
    internal sealed class ConstantAdapter<T> : AdapterMember<T>
    {
        private readonly Converter<T> converter;

        public ConstantAdapter(Converter<T> converter) => this.converter = converter;

        public override void Of(ref Allocator allocator, in ReadOnlySpan<T> span)
        {
            for (var i = 0; i < span.Length; i++)
                converter.ToBytes(ref allocator, span[i]);
            Debug.Assert(converter.Length > 0);
        }

        public override ArraySegment<T> To(in ReadOnlySpan<byte> span)
        {
            Debug.Assert(converter.Length > 0);
            var byteCount = span.Length;
            if (byteCount == 0)
                return new ArraySegment<T>(Array.Empty<T>());
            var definition = converter.Length;
            var itemCount = CollectionHelper.GetItemCount(byteCount, definition);
            var items = new T[itemCount];
            for (var i = 0; i < itemCount; i++)
                items[i] = converter.ToValue(span.Slice(i * definition));
            return new ArraySegment<T>(items);
        }
    }
}
