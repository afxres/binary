namespace Mikodev.Binary.SourceGeneration.NamedObjectTests.CircularTypeReferenceTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<LinkedList<int>>]
[SourceGeneratorInclude<LinkedList<long>>]
public partial class LinkedListGeneratorContext { }

[NamedObject]
public class LinkedList<T>(T data, LinkedList<T>? next)
{
    [NamedKey("data")]
    public T Data = data;

    [NamedKey("next")]
    public LinkedList<T>? Next = next;

    public LinkedList<T> Add(T data) => new LinkedList<T>(data, this);

    public LinkedList(T data) : this(data, null) { }

    public IEnumerable<T> Enumerate()
    {
        for (var next = this; next is not null; next = next.Next)
            yield return next.Data;
    }
}

public class LinkedListTests
{
    [Fact(DisplayName = "Custom Linked List Test")]
    public void LinkedListTest()
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(LinkedListGeneratorContext.ConverterCreators.Values)
            .Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<LinkedList<int>>();
        var converterSecond = generatorSecond.GetConverter<LinkedList<int>>();
        Assert.NotEqual(converter.GetType(), converterSecond.GetType());

        var source = new LinkedList<int>(3).Add(2).Add(1);
        var buffer = converter.Encode(source);
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(buffer, bufferSecond);

        var result = converter.Decode(buffer);
        var resultSecond = converterSecond.Decode(bufferSecond);
        var actual = result.Enumerate().ToList();
        var actualSecond = resultSecond.Enumerate().ToList();
        Assert.Equal([1, 2, 3], actual);
        Assert.Equal([1, 2, 3], actualSecond);
    }

    [Fact(DisplayName = "Custom Linked List With Cycle Test")]
    public void LinkedListWithCycleTest()
    {
        var head = new LinkedList<long>(1);
        var tail = new LinkedList<long>(3);
        head.Next = tail;
        tail.Next = head;

        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(LinkedListGeneratorContext.ConverterCreators.Values)
            .Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<LinkedList<long>>();
        var converterSecond = generatorSecond.GetConverter<LinkedList<long>>();
        Assert.NotEqual(converter.GetType(), converterSecond.GetType());

        _ = Assert.Throws<InsufficientExecutionStackException>(() => converter.Encode(head));
        _ = Assert.Throws<InsufficientExecutionStackException>(() => converterSecond.Encode(head));
    }
}
