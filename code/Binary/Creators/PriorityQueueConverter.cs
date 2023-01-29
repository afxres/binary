namespace Mikodev.Binary.Creators;

using System;
using System.Collections.Generic;

internal sealed class PriorityQueueConverter<E, P> : Converter<PriorityQueue<E, P>>
{
    private readonly Converter<E> init;

    private readonly Converter<P> tail;

    public PriorityQueueConverter(Converter<E> init, Converter<P> tail)
    {
        this.init = init;
        this.tail = tail;
    }

    public override void Encode(ref Allocator allocator, PriorityQueue<E, P>? item)
    {
        if (item is null)
            return;
        var init = this.init;
        var tail = this.tail;
        foreach (var (head, next) in item.UnorderedItems)
        {
            init.EncodeAuto(ref allocator, head);
            tail.EncodeAuto(ref allocator, next);
        }
    }

    public override PriorityQueue<E, P> Decode(in ReadOnlySpan<byte> span)
    {
        var init = this.init;
        var tail = this.tail;
        var result = new PriorityQueue<E, P>();
        var intent = span;
        while (intent.Length is not 0)
        {
            var head = init.DecodeAuto(ref intent);
            var next = tail.DecodeAuto(ref intent);
            result.Enqueue(head, next);
        }
        return result;
    }
}
