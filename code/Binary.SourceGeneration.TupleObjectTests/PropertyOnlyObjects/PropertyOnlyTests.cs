namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.PropertyOnlyObjects;

using Mikodev.Binary;
using Mikodev.Binary.Attributes;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<Person>]
[SourceGeneratorInclude<Student>]
public partial class PropertyOnlySourceGeneratorContext { }

[TupleObject]
public class Person
{
    [TupleKey(0)]
    public required int Id { get; init; }

    [TupleKey(1)]
    public required string Name { get; init; }
}

[TupleObject]
public class Student
{
    [TupleKey(2)]
    public required string Id { get; init; }

    [TupleKey(0)]
    public required int Age { get; init; }

    [TupleKey(1)]
    public required string Name { get; init; }

    [TupleKey(3)]
    public required string Class { get; init; }
}

public class PropertyOnlyTests
{
    [Fact(DisplayName = "Encode Decode Ordered Members")]
    public void OrderedMembers()
    {
        var builder = Generator.CreateDefaultBuilder();
        foreach (var i in PropertyOnlySourceGeneratorContext.ConverterCreators)
            _ = builder.AddConverterCreator(i.Value);
        var generator = builder.Build();
        var converter = generator.GetConverter<Person>();
        Assert.True(converter.GetType().Assembly == typeof(Person).Assembly);
        var source = new Person { Id = 100, Name = "Nice!" };
        var expectedSource = (100, "Nice!");
        var buffer = converter.Encode(source);
        var bufferExpected = generator.Encode(expectedSource);
        Assert.Equal(bufferExpected, buffer);
        var result = converter.Decode(buffer);
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.Name, result.Name);
    }

    [Fact(DisplayName = "Encode Decode Unordered Members")]
    public void UnorderedMembers()
    {
        var builder = Generator.CreateDefaultBuilder();
        foreach (var i in PropertyOnlySourceGeneratorContext.ConverterCreators)
            _ = builder.AddConverterCreator(i.Value);
        var generator = builder.Build();
        var converter = generator.GetConverter<Student>();
        Assert.Equal(converter.GetType().Assembly, typeof(PropertyOnlySourceGeneratorContext).Assembly);
        var source = new Student { Age = 20, Class = "C-2", Id = "A8402", Name = "Tom" };
        var expectedSource = (20, "Tom", "A8402", "C-2");
        var buffer = converter.Encode(source);
        var bufferExpected = generator.Encode(expectedSource);
        Assert.Equal(bufferExpected, buffer);
        var result = converter.Decode(buffer);
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.Age, result.Age);
        Assert.Equal(source.Name, result.Name);
        Assert.Equal(source.Class, result.Class);
    }
}
