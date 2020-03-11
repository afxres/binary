using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class ArrayLikeVariableAdapter<T> : ArrayLikeAdapter<T>
    {
        private readonly Converter<T> converter;

        public ArrayLikeVariableAdapter(Converter<T> converter) => this.converter = converter;

        public override void Of(ref Allocator allocator, ReadOnlyMemory<T> memory)
        {
            foreach (var i in memory.Span)
                converter.EncodeAuto(ref allocator, i);
            Debug.Assert(converter.Length == 0);
        }

        public override ArraySegment<T> To(ReadOnlySpan<byte> span)
        {
            static void Expand(ref T[] buffer, T item)
            {
                var source = buffer;
                var cursor = source.Length;
                buffer = new T[checked(cursor * 2)];
                Array.Copy(source, 0, buffer, 0, cursor);
                buffer[cursor] = item;
            }

            Debug.Assert(converter.Length == 0);
            if (span.IsEmpty)
                return new ArraySegment<T>(Array.Empty<T>());
            const int Initial = 8;
            var buffer = new T[Initial];
            var cursor = 0;
            var body = span;
            while (!body.IsEmpty)
            {
                var item = converter.DecodeAuto(ref body);
                if ((uint)cursor < (uint)buffer.Length)
                    buffer[cursor] = item;
                else
                    Expand(ref buffer, item);
                cursor++;
            }
            return new ArraySegment<T>(buffer, 0, cursor);
        }
    }
}
