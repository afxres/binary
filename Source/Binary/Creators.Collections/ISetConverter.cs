using Mikodev.Binary.Converters.Abstractions;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ISetConverter<TCollection, T> : CollectionConverter<TCollection, T> where TCollection : IEnumerable<T>
    {
        public ISetConverter(Converter<T> converter) : base(converter, reverse: false) { }

        public override TCollection ToValue(in ReadOnlySpan<byte> span) => (TCollection)(object)new HashSet<T>(GetCollection(span));
    }
}
