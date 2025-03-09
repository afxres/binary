namespace Mikodev.Binary.Tests.Implementations;

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class CircularTypeReferenceTests
{
    private class LinkedList<T>(T data, LinkedList<T>? next)
    {
        public T Data = data;

        public LinkedList<T>? Next = next;

        public LinkedList<T> Add(T data) => new LinkedList<T>(data, this);

        public LinkedList(T data) : this(data, null) { }

        public IEnumerable<T> Enumerate()
        {
            for (var next = this; next is not null; next = next.Next)
                yield return next.Data;
        }
    }

    private class ObjectBox
    {
        public object? Data;
    }

    private class ObjectBoxConverter(Converter<object?> converter) : Converter<ObjectBox>
    {
        public override void Encode(ref Allocator allocator, ObjectBox? item)
        {
            Assert.NotNull(item);
            Assert.NotNull(item.Data);
            converter.Encode(ref allocator, item.Data);
        }

        public override ObjectBox Decode(in ReadOnlySpan<byte> span)
        {
            throw new NotSupportedException();
        }
    }

    private class ObjectBoxConverterCreator : IConverterCreator
    {
        public IConverter? GetConverter(IGeneratorContext context, Type type)
        {
            return new ObjectBoxConverter((Converter<object?>)context.GetConverter(typeof(object)));
        }
    }

    private class A
    {
        public B? Data { get; set; }
    }

    private class B
    {
        public C? Data { get; set; }
    }

    private class C
    {
        public A? Data { get; set; }
    }

    [Fact(DisplayName = "Custom Linked List")]
    public void CustomLinkedList()
    {
        // 6 -> 4 -> 2 -> null
        var list = new LinkedList<int>(2).Add(4).Add(6);
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<LinkedList<int>>();
        var buffer = converter.Encode(list);
        var result = converter.Decode(buffer);
        var actual = result.Enumerate().ToList();
        Assert.Equal([6, 4, 2], actual);
    }

    [Fact(DisplayName = "Custom Linked List With Cycle")]
    public void CustomLinkedListWithCycle()
    {
        // 1 <-> 3
        var head = new LinkedList<long>(1);
        var tail = new LinkedList<long>(3);
        head.Next = tail;
        tail.Next = head;
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<LinkedList<long>>();
        _ = Assert.Throws<InsufficientExecutionStackException>(() => converter.Encode(head));
    }

    [Fact(DisplayName = "Custom Object Box With Cycle")]
    public void CustomObjectBoxWithCycle()
    {
        var head = new ObjectBox();
        var tail = new ObjectBox();
        head.Data = tail;
        tail.Data = head;
        var generator = Generator.CreateDefaultBuilder().AddConverterCreator(new ObjectBoxConverterCreator()).Build();
        var converter = generator.GetConverter<ObjectBox>();
        _ = Assert.Throws<InsufficientExecutionStackException>(() => converter.Encode(head));
    }

    [Fact(DisplayName = "Circular Type Reference")]
    public void CircularTypeReference()
    {
        var data = new A();
        var generator = Generator.CreateDefault();
        var error = Assert.Throws<ArgumentException>(() => generator.Encode(data));
        var message = $"Circular type reference detected, type: {typeof(A)}";
        Assert.Equal(message, error.Message);
    }
}
