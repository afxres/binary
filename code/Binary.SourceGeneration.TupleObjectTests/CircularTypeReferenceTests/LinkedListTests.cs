namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.CircularTypeReferenceTests;

using Mikodev.Binary.Attributes;
using System;
using Xunit;

[TupleObject]
public class LinkedList<T>(T data, LinkedList<T>? next)
{
    [TupleKey(0)]
    public T Data = data;

    [TupleKey(1)]
    public LinkedList<T>? Next = next;
}

public class LinkedListTests
{
    [Fact(DisplayName = "Linked List Circular Type Reference Test")]
    public void LinkedListCircularTypeReferenceTest()
    {
        var generatorSecond = Generator.CreateDefault();
        var b = Assert.Throws<ArgumentException>(generatorSecond.GetConverter<LinkedList<long>>);
        Assert.Equal($"Self type reference detected, type: {typeof(LinkedList<long>)}", b.Message);
    }
}
