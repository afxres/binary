using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Components;
using Mikodev.Binary.Internal.Delegates;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Converters.Runtime.Collections
{
    internal sealed class GenericDictionaryConverter<T, K, V> : VariableConverter<T> where T : IEnumerable<KeyValuePair<K, V>>
    {
        private readonly DictionaryConverter<T, K, V> converter;

        private readonly ToDictionary<T, K, V> constructor;

        public GenericDictionaryConverter(ToDictionary<T, K, V> constructor, Converter<KeyValuePair<K, V>> converter)
        {
            this.constructor = constructor;
            this.converter = new DictionaryConverter<T, K, V>(converter);
        }

        public override void ToBytes(ref Allocator allocator, T item)
        {
            converter.Of(ref allocator, item);
        }

        public override T ToValue(in ReadOnlySpan<byte> span)
        {
            if (constructor == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var source = converter.To(in span);
            return constructor.Invoke(source);
        }
    }
}
