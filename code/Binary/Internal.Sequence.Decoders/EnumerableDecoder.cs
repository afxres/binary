namespace Mikodev.Binary.Internal.Sequence.Decoders;

using Mikodev.Binary.Internal.Sequence;
using System;
using System.Collections.Generic;
using System.Diagnostics;

internal sealed class EnumerableDecoder<T, E> where T : IEnumerable<E>
{
    private readonly SequenceAdapter<E> invoke;

    public EnumerableDecoder(Converter<E> converter)
    {
        Debug.Assert(converter is not null);
        Debug.Assert(converter.Length >= 0);
        this.invoke = SequenceAdapter.Create(converter);
    }

    public T Decode(ReadOnlySpan<byte> span)
    {
        return (T)this.invoke.Decode(span).GetEnumerable();
    }
}
