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

    public override void Encode(ref Allocator allocator, PriorityQueue<E, P> item)
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
        var item = new PriorityQueue<E, P>();
        var init = this.init;
        var tail = this.tail;
        var body = span;
        while (body.Length is not 0)
        {
            var head = init.DecodeAuto(ref body);
            var next = tail.DecodeAuto(ref body);
            item.Enqueue(head, next);
        }
        return item;
    }
}
