using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.SpanLike.Adapters
{
    internal sealed class SpanLikeVariableAdapter<T> : SpanLikeAdapter<T>
    {
        private readonly Converter<T> converter;

        public SpanLikeVariableAdapter(Converter<T> converter) => this.converter = converter;

        public override void Encode(ref Allocator allocator, ReadOnlySpan<T> item)
        {
            foreach (var i in item)
                converter.EncodeAuto(ref allocator, i);
            Debug.Assert(converter.Length is 0);
        }

        public override MemoryResult<T> Decode(ReadOnlySpan<byte> span)
        {
            static void Expand(ref T[] buffer, T item)
            {
                var source = buffer;
                var cursor = source.Length;
                buffer = new T[checked(cursor * 2)];
                Array.Copy(source, 0, buffer, 0, cursor);
                buffer[cursor] = item;
            }

            Debug.Assert(converter.Length is 0);
            if (span.IsEmpty)
                return new MemoryResult<T>(Array.Empty<T>(), 0);
            const int Initial = 8;
            var buffer = new T[Initial];
            var cursor = 0;
            var body = span;
            while (body.IsEmpty is false)
            {
                var item = converter.DecodeAuto(ref body);
                if ((uint)cursor < (uint)buffer.Length)
                    buffer[cursor] = item;
                else
                    Expand(ref buffer, item);
                cursor++;
            }
            return new MemoryResult<T>(buffer, cursor);
        }
    }
}
