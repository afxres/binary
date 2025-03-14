namespace Mikodev.Binary.SourceGeneration.NamedObjectTests.CircularTypeReferenceTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<LinkedList<int>>]
[SourceGeneratorInclude<LinkedList<long>>]
[SourceGeneratorInclude<CustomLinkedListWithCustomConverterAttribute>]
[SourceGeneratorInclude<CustomLinkedListWithCustomConverterCreatorAttribute>]
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

[NamedObject]
public class CustomLinkedListWithCustomConverterAttribute
{
    [NamedKey("item")]
    public string? Data;

    [NamedKey("next")]
    [Converter(typeof(CustomLinkedListWithCustomConverterAttributeConverter))]
    public CustomLinkedListWithCustomConverterAttribute? Next;
}

public class CustomLinkedListWithCustomConverterAttributeConverter : Converter<CustomLinkedListWithCustomConverterAttribute>
{
    public override void Encode(ref Allocator allocator, CustomLinkedListWithCustomConverterAttribute? item)
    {
        Allocator.Append(ref allocator, $"Data = {item?.Data}", Encoding.UTF8);
    }

    public override CustomLinkedListWithCustomConverterAttribute Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
}

[NamedObject]
public class CustomLinkedListWithCustomConverterCreatorAttribute
{
    [NamedKey("data")]
    public string? Data;

    [NamedKey("node")]
    [ConverterCreator(typeof(CustomLinkedListWithCustomConverterCreatorAttributeConverterCreator))]
    public CustomLinkedListWithCustomConverterCreatorAttribute? Next;
}

public class CustomLinkedListWithCustomConverterCreatorAttributeConverter(Converter<string> converter) : Converter<CustomLinkedListWithCustomConverterCreatorAttribute>
{
    public override void Encode(ref Allocator allocator, CustomLinkedListWithCustomConverterCreatorAttribute? item)
    {
        converter.Encode(ref allocator, $"Item = {item?.Data}");
    }

    public override CustomLinkedListWithCustomConverterCreatorAttribute Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
}

public class CustomLinkedListWithCustomConverterCreatorAttributeConverterCreator : IConverterCreator
{
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        if (type != typeof(CustomLinkedListWithCustomConverterCreatorAttribute))
            return null;
        return new CustomLinkedListWithCustomConverterCreatorAttributeConverter(context.GetConverter<string>());
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

    [Fact(DisplayName = "Custom Linked List With Custom Converter Attribute Test")]
    public void CustomLinkedListWithCustomConverterAttributeTest()
    {
        var tail = new CustomLinkedListWithCustomConverterAttribute { Data = "bravo" };
        var head = new CustomLinkedListWithCustomConverterAttribute { Data = "alpha", Next = tail };
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(LinkedListGeneratorContext.ConverterCreators.Values)
            .Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<CustomLinkedListWithCustomConverterAttribute>();
        var converterSecond = generatorSecond.GetConverter<CustomLinkedListWithCustomConverterAttribute>();
        Assert.NotEqual(converter.GetType(), converterSecond.GetType());

        var buffer = converter.Encode(head);
        var bufferSecond = converterSecond.Encode(head);
        Assert.Equal(buffer, bufferSecond);

        var token = new Token(generator, buffer);
        Assert.Equal("alpha", token["item"].As<string>());
        Assert.Equal("Data = bravo", token["next"].As<string>());
    }

    [Fact(DisplayName = "Custom Linked List With Custom Converter Creator Attribute Test")]
    public void CustomLinkedListWithCustomConverterCreatorAttributeTest()
    {
        var tail = new CustomLinkedListWithCustomConverterCreatorAttribute { Data = "Delta" };
        var head = new CustomLinkedListWithCustomConverterCreatorAttribute { Data = "Alice", Next = tail };
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(LinkedListGeneratorContext.ConverterCreators.Values)
            .Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<CustomLinkedListWithCustomConverterCreatorAttribute>();
        var converterSecond = generatorSecond.GetConverter<CustomLinkedListWithCustomConverterCreatorAttribute>();
        Assert.NotEqual(converter.GetType(), converterSecond.GetType());

        var buffer = converter.Encode(head);
        var bufferSecond = converterSecond.Encode(head);
        Assert.Equal(buffer, bufferSecond);

        var token = new Token(generator, buffer);
        Assert.Equal("Alice", token["data"].As<string>());
        Assert.Equal("Item = Delta", token["node"].As<string>());
    }
}
