using Mikodev.Binary.Converters.Abstractions;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IEnumerableConverter<TCollection, T> : CollectionConverter<TCollection, T> where TCollection : IEnumerable<T>
    {
        public IEnumerableConverter(Converter<T> converter) : base(converter, reverse: false) { }

        public override TCollection ToValue(in ReadOnlySpan<byte> span) => (TCollection)(object)To(in span);
    }
}
