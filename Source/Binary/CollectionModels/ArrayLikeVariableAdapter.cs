using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;

namespace Mikodev.Binary.CollectionModels
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

        public override MemoryItem<T> To(in ReadOnlySpan<byte> span)
        {
            Debug.Assert(converter.Length == 0);
            var byteCount = span.Length;
            if (byteCount == 0)
                return new MemoryItem<T>(Array.Empty<T>(), 0);
            const int InitialCapacity = 8;
            var buffer = new T[InitialCapacity];
            var limits = (long)InitialCapacity;
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
                buffer[cursor++] = converter.DecodeAuto(ref temp);
            }
            Debug.Assert(cursor <= buffer.Length);
            return new MemoryItem<T>(buffer, (int)cursor);
        }
    }
}
