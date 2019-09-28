using Mikodev.Binary.Adapters.Abstractions;
using System;
using System.Diagnostics;

namespace Mikodev.Binary.Adapters.Implementations
{
    internal sealed class VariableAdapter<T> : AdapterMember<T>
    {
        private readonly Converter<T> converter;

        public VariableAdapter(Converter<T> converter) => this.converter = converter;

        public override void Of(ref Allocator allocator, in ReadOnlySpan<T> span)
        {
            for (var i = 0; i < span.Length; i++)
                converter.ToBytesWithMark(ref allocator, span[i]);
            Debug.Assert(converter.Length == 0);
        }

        public override ArraySegment<T> To(in ReadOnlySpan<byte> span)
        {
            Debug.Assert(converter.Length == 0);
            var byteCount = span.Length;
            if (byteCount == 0)
                return new ArraySegment<T>(Array.Empty<T>());
            const int InitialLength = 8;
            var buffer = new T[InitialLength];
            var limits = (long)InitialLength;
            var cursor = 0L;
            var temp = span;
            while (!temp.IsEmpty)
            {
                if (cursor >= limits)
                {
                    Debug.Assert(cursor > 0 && cursor == buffer.Length);
                    var target = new T[checked((int)(limits *= 2))];
                    MemoryExtensions.CopyTo(buffer, (Span<T>)target);
                    buffer = target;
                }
                buffer[cursor++] = converter.ToValueWithMark(ref temp);
            }
            Debug.Assert(cursor <= buffer.Length);
            return new ArraySegment<T>(buffer, 0, (int)cursor);
        }
    }
}
