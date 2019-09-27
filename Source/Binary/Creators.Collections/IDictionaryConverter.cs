using Mikodev.Binary.Converters.Abstractions;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IDictionaryConverter<TDictionary, TIndex, TValue> : DictionaryConverter<TDictionary, TIndex, TValue> where TDictionary : IEnumerable<KeyValuePair<TIndex, TValue>>
    {
        public IDictionaryConverter(Converter<TIndex> indexConverter, Converter<TValue> valueConverter) : base(indexConverter, valueConverter) { }

        public override TDictionary ToValue(in ReadOnlySpan<byte> span) => (TDictionary)(object)To(in span);
    }
}
