using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Sequence.Decoders
{
    internal sealed class LinkedListDecoder<E> : SequenceDecoder<LinkedList<E>>
    {
        private readonly Converter<E> converter;

        public LinkedListDecoder(Converter<E> converter) => this.converter = converter;

        public override LinkedList<E> Decode(ReadOnlySpan<byte> span)
        {
            var body = span;
            var list = new LinkedList<E>();
            var converter = this.converter;
            while (body.IsEmpty is false)
                _ = list.AddLast(converter.DecodeAuto(ref body));
            return list;
        }
    }
}
