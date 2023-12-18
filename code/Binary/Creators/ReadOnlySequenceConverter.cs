namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Buffers;

internal sealed class ReadOnlySequenceConverter<E>(Converter<E> converter) : Converter<ReadOnlySequence<E>>
{
    private readonly Converter<E> converter = converter;

    public override void Encode(ref Allocator allocator, ReadOnlySequence<E> item)
    {
        var converter = this.converter;
        foreach (var memory in item)
            foreach (var i in memory.Span)
                converter.EncodeAuto(ref allocator, i);
        return;
    }

    public override ReadOnlySequence<E> Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return ReadOnlySequence<E>.Empty;
        var result = SpanLikeMethods.GetArray(this.converter, span, out var actual);
        return new ReadOnlySequence<E>(result, 0, actual);
    }
}
