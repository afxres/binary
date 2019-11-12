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
            var span = memory.Span;
            for (var i = 0; i < span.Length; i++)
                converter.EncodeAuto(ref allocator, span[i]);
            Debug.Assert(converter.Length == 0);
        }

        public override MemoryItem<T> To(ReadOnlySpan<byte> span)
        {
            Debug.Assert(converter.Length == 0);
            if (span.IsEmpty)
                return new MemoryItem<T>(Array.Empty<T>(), 0);
            const int Initial = 8;
            var buffer = new T[Initial];
            var bounds = Initial;
            var cursor = 0;
            var body = span;
            while (!body.IsEmpty)
            {
                Debug.Assert(cursor >= 0);
                Debug.Assert(bounds == buffer.Length);
                if (cursor == bounds)
                {
                    var length = checked(bounds * 2);
                    var target = new T[length];
                    MemoryExtensions.CopyTo(buffer, target.AsSpan());
                    bounds = length;
                    buffer = target;
                }
                buffer[cursor++] = converter.DecodeAuto(ref body);
            }
            Debug.Assert(cursor <= buffer.Length);
            return new MemoryItem<T>(buffer, cursor);
        }
    }
}
