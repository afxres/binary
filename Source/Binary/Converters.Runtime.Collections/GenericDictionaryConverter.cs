using Mikodev.Binary.Converters.Abstractions;
using Mikodev.Binary.Delegates;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Converters.Runtime.Collections
{
    internal sealed class GenericDictionaryConverter<TCollection, TIndex, TValue> : DictionaryConverter<TCollection, TIndex, TValue> where TCollection : IEnumerable<KeyValuePair<TIndex, TValue>>
    {
        private readonly ToDictionary<TCollection, TIndex, TValue> constructor;

        public GenericDictionaryConverter(Converter<TIndex> indexConverter, Converter<TValue> valueConverter, ToDictionary<TCollection, TIndex, TValue> constructor) : base(indexConverter, valueConverter)
        {
            this.constructor = constructor;
        }

        public override TCollection ToValue(in ReadOnlySpan<byte> span)
        {
            if (constructor == null)
                return ThrowHelper.ThrowNoSuitableConstructor<TCollection>();
            var source = To(in span);
            return constructor.Invoke(source);
        }
    }
}
