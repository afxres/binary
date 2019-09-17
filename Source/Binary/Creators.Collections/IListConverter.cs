using Mikodev.Binary.Converters.Abstractions;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class IListConverter<TCollection, T> : CollectionConverter<TCollection, T> where TCollection : IEnumerable<T>
    {
        public IListConverter(Converter<T> converter) : base(converter, reverse: false) { }

        public override TCollection ToValue(in ReadOnlySpan<byte> span) => (TCollection)GetCollection(span);
    }
}
