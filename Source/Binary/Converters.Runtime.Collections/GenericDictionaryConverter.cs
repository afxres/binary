using Mikodev.Binary.Converters.Abstractions;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Converters.Runtime.Collections
{
    internal sealed class GenericDictionaryConverter<T, K, V> : DictionaryConverter<T, K, V> where T : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly ToDictionary<T, K, V> constructor;

        public GenericDictionaryConverter(Converter<KeyValuePair<K, V>> converter, ToDictionary<T, K, V> constructor) : base(converter)
        {
            this.constructor = constructor;
        }

        public override T ToValue(in ReadOnlySpan<byte> span)
        {
            if (constructor == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var source = To(in span);
            return constructor.Invoke(source);
        }
    }
}
