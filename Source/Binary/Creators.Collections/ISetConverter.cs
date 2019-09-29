using Mikodev.Binary.Converters.Abstractions;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Collections
{
    internal sealed class ISetConverter<R, E> : CollectionConverter<R, E> where R : IEnumerable<E>
    {
        public ISetConverter(Converter<E> converter) : base(converter, reverse: false) { }

        public override R ToValue(in ReadOnlySpan<byte> span) => (R)(object)new HashSet<E>(To(in span));
    }
}
