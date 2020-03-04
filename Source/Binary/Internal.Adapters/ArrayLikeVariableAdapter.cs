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
            Debug.Assert(converter.Length == 0);
            if (span.IsEmpty)
                return new ArraySegment<T>(Array.Empty<T>());
            const int Initial = 8;
            var buffer = new T[Initial];
            var bounds = Initial;
            var cursor = 0;
            var body = span;
            while (!body.IsEmpty)
            {
                if (cursor == bounds)
                {
                    var source = buffer;
                    bounds = checked(bounds * 2);
                    buffer = new T[bounds];
                    Array.Copy(source, buffer, source.Length);
                }
                buffer[cursor++] = converter.DecodeAuto(ref body);
            }
            return new ArraySegment<T>(buffer, 0, cursor);
        }
    }
}
