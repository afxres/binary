using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Sequence.Decoders
{
    internal sealed class HashSetDecoder<E> : SequenceDecoder<HashSet<E>>
    {
        private readonly Converter<E> converter;

        public HashSetDecoder(Converter<E> converter) => this.converter = converter;

        public override HashSet<E> Decode(ReadOnlySpan<byte> span)
        {
            var body = span;
            var item = new HashSet<E>();
            var converter = this.converter;
            while (body.Length is not 0)
                _ = item.Add(converter.DecodeAuto(ref body));
            return item;
        }
    }
}
