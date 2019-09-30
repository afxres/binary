using Microsoft.FSharp.Collections;
using Mikodev.Binary.Abstractions;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.External.Collections
{
    internal sealed class FSharpMapConverter<K, V> : VariableConverter<FSharpMap<K, V>>
    {
        private readonly Converter<KeyValuePair<K, V>> converter;

        public FSharpMapConverter(Converter<KeyValuePair<K, V>> converter) => this.converter = converter;

        public override void ToBytes(ref Allocator allocator, FSharpMap<K, V> item)
        {
            if (item == null)
                return;
            var converter = this.converter;
            foreach (var i in item)
                converter.ToBytesWithMark(ref allocator, i);
        }

        public override FSharpMap<K, V> ToValue(in ReadOnlySpan<byte> span)
        {
            static FSharpMap<K, V> Add(FSharpMap<K, V> data, KeyValuePair<K, V> item) => data.Add(item.Key, item.Value);

            var temp = span;
            var data = MapModule.Empty<K, V>();
            var converter = this.converter;
            while (!temp.IsEmpty)
                data = Add(data, converter.ToValueWithMark(ref temp));
            return data;
        }
    }
}
