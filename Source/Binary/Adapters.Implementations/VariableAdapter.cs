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
            var buffer = Array.Empty<T>();
            var cursor = 0L;
            var limits = 0L;

            void Ensure()
            {
                Debug.Assert(cursor >= 0);
                Debug.Assert(limits >= 0);
                limits = limits == 0 ? 4 : limits * 2;
                var target = new T[checked((int)limits)];
                if (cursor != 0)
                    Array.Copy(buffer, target, cursor);
                buffer = target;
            }

            var temp = span;
            while (!temp.IsEmpty)
            {
                if (cursor >= limits)
                    Ensure();
                var item = converter.ToValueWithMark(ref temp);
                buffer[cursor] = item;
                cursor += 1;
            }

            Debug.Assert(cursor <= buffer.Length);
            return new ArraySegment<T>(buffer, 0, (int)cursor);
        }
    }
}
