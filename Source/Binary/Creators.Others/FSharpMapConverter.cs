using Microsoft.FSharp.Collections;
using Mikodev.Binary.Abstractions;
using System;

namespace Mikodev.Binary.Creators.Others
{
    internal sealed class FSharpMapConverter<TIndex, TValue> : VariableConverter<FSharpMap<TIndex, TValue>>
    {
        private readonly Converter<TIndex> indexConverter;

        private readonly Converter<TValue> valueConverter;

        public FSharpMapConverter(Converter<TIndex> indexConverter, Converter<TValue> valueConverter)
        {
            this.indexConverter = indexConverter;
            this.valueConverter = valueConverter;
        }

        public override void ToBytes(ref Allocator allocator, FSharpMap<TIndex, TValue> item)
        {
            if (item == null)
                return;
            foreach (var i in item)
            {
                indexConverter.ToBytesWithMark(ref allocator, i.Key);
                valueConverter.ToBytesWithMark(ref allocator, i.Value);
            }
        }

        public override FSharpMap<TIndex, TValue> ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            var item = MapModule.Empty<TIndex, TValue>();
            while (!temp.IsEmpty)
            {
                var index = indexConverter.ToValueWithMark(ref temp);
                var value = valueConverter.ToValueWithMark(ref temp);
                item = item.Add(index, value);
            }
            return item;
        }
    }
}
