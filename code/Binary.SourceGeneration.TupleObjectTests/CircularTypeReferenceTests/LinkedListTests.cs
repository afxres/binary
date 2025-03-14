namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.CircularTypeReferenceTests;

using Mikodev.Binary.Attributes;
using System;
using System.Text;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<CustomLinkedListWithCustomConverterAttribute>]
[SourceGeneratorInclude<CustomLinkedListWithCustomConverterCreatorAttribute>]
public partial class LinkedListGeneratorContext { }

[TupleObject]
public class LinkedList<T>(T data, LinkedList<T>? next)
{
    [TupleKey(0)]
    public T Data = data;

    [TupleKey(1)]
    public LinkedList<T>? Next = next;
}

[TupleObject]
public class CustomLinkedListWithCustomConverterAttribute
{
    [TupleKey(0)]
    public string? Data;

    [TupleKey(1)]
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

[TupleObject]
public class CustomLinkedListWithCustomConverterCreatorAttribute
{
    [TupleKey(1)]
    public string? Data;

    [TupleKey(0)]
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
    [Fact(DisplayName = "Linked List Circular Type Reference Test")]
    public void LinkedListCircularTypeReferenceTest()
    {
        var generatorSecond = Generator.CreateDefault();
        var b = Assert.Throws<ArgumentException>(generatorSecond.GetConverter<LinkedList<long>>);
        Assert.Equal($"Self type reference detected, type: {typeof(LinkedList<long>)}", b.Message);
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

        var bufferExpected = generatorSecond.Encode(("alpha", "Data = bravo"));
        var buffer = converter.Encode(head);
        var bufferSecond = converterSecond.Encode(head);
        Assert.Equal(bufferExpected, buffer);
        Assert.Equal(bufferExpected, bufferSecond);
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

        var bufferExpected = generatorSecond.Encode(("Item = Delta", "Alice"));
        var buffer = converter.Encode(head);
        var bufferSecond = converterSecond.Encode(head);
        Assert.Equal(bufferExpected, buffer);
        Assert.Equal(bufferExpected, bufferSecond);
    }
}
