using Mikodev.Binary.Converters.Abstractions;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections.Common
{
    internal sealed class IDictionaryConverter<T, K, V> : DictionaryConverter<T, K, V> where T : IEnumerable<KeyValuePair<K, V>>
    {
        public IDictionaryConverter(Converter<KeyValuePair<K, V>> converter) : base(converter) { }

        public override T ToValue(in ReadOnlySpan<byte> span) => (T)(object)To(in span);
    }
}
