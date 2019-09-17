using Mikodev.Binary.Adapters.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Adapters
{
    internal sealed class VariableAdapter<T> : Adapter<T>
    {
        private readonly Converter<T> converter;

        public VariableAdapter(Converter<T> converter) => this.converter = converter;

        public override void OfArray(ref Allocator allocator, in ReadOnlySpan<T> span)
        {
            Debug.Assert(converter.Length == 0);
            for (var i = 0; i < span.Length; i++)
                converter.ToBytesWithMark(ref allocator, span[i]);
        }

        public override T[] ToArray(in ReadOnlySpan<byte> span) => ToValue(span).ToArray();

        public override List<T> ToValue(in ReadOnlySpan<byte> span)
        {
            Debug.Assert(converter.Length == 0);
            var list = new List<T>();
            var temp = span;
            while (!temp.IsEmpty)
                list.Add(converter.ToValueWithMark(ref temp));
            return list;
        }
    }
}
