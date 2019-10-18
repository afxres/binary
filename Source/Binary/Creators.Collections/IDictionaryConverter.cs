using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal.Components;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IDictionaryConverter<T, K, V> : VariableConverter<T> where T : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly DictionaryConverter<T, K, V> converter;

        public IDictionaryConverter(Converter<KeyValuePair<K, V>> converter) => this.converter = new DictionaryConverter<T, K, V>(converter);

        public override void ToBytes(ref Allocator allocator, T item) => converter.Of(ref allocator, item);

        public override T ToValue(in ReadOnlySpan<byte> span) => (T)(object)converter.To(in span);
    }
}
