using Mikodev.Binary.Internal.Sequence;
using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Adapters
{
    internal sealed class SpanLikeConstantAdapter<T> : SpanLikeAdapter<T>
    {
        private readonly Converter<T> converter;

        public SpanLikeConstantAdapter(Converter<T> converter) => this.converter = converter;

        public override void Encode(ref Allocator allocator, ReadOnlySpan<T> item)
        {
            foreach (var i in item)
                converter.Encode(ref allocator, i);
            Debug.Assert(converter.Length > 0);
        }

        public override MemoryResult<T> Decode(ReadOnlySpan<byte> span)
        {
            Debug.Assert(converter.Length > 0);
            var byteLength = span.Length;
            if (byteLength == 0)
                return new MemoryResult<T>(Array.Empty<T>(), 0);
            var itemLength = converter.Length;
            var capacity = SequenceMethods.GetCapacity<T>(byteLength, itemLength);
            var collection = new T[capacity];
            for (var i = 0; i < capacity; i++)
                collection[i] = converter.Decode(span.Slice(i * itemLength, itemLength));
            return new MemoryResult<T>(collection, capacity);
        }
    }
}
