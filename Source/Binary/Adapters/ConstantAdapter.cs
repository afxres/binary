using Mikodev.Binary.Adapters.Abstractions;
using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;

namespace Mikodev.Binary.Adapters
{
    internal sealed class ConstantAdapter<T> : Adapter<T>
    {
        private readonly Converter<T> converter;

        public ConstantAdapter(Converter<T> converter) => this.converter = converter;

        public override void OfArray(ref Allocator allocator, in ReadOnlySpan<T> span)
        {
            Debug.Assert(converter.Length > 0);
            for (var i = 0; i < span.Length; i++)
                converter.ToBytes(ref allocator, span[i]);
        }

        public override T[] ToArray(in ReadOnlySpan<byte> span)
        {
            Debug.Assert(converter.Length > 0);
            var byteCount = span.Length;
            if (byteCount == 0)
                return Array.Empty<T>();
            var definition = converter.Length;
            var itemCount = Define.GetItemCount(byteCount, definition);
            var array = new T[itemCount];
            for (var i = 0; i < itemCount; i++)
                array[i] = converter.ToValue(span.Slice(i * definition));
            return array;
        }
    }
}
