namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.Constructors;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<NoPublicConstructorNamedObject>]
[SourceGeneratorInclude<NoPublicConstructorTupleObject>]
public partial class NoPublicConstructorGeneratorContext { }

[NamedObject]
public class NoPublicConstructorNamedObject
{
    [NamedKey("id")]
    public int Id { get; set; }

    private NoPublicConstructorNamedObject() => throw new NotSupportedException();

    internal NoPublicConstructorNamedObject(int id) => Id = id;

    public static NoPublicConstructorNamedObject Create(int id) => new NoPublicConstructorNamedObject(id);
}

[TupleObject]
public class NoPublicConstructorTupleObject
{
    [TupleKey(0)]
    public string? Name { get; set; }

    private NoPublicConstructorTupleObject() => throw new NotSupportedException();

    internal NoPublicConstructorTupleObject(string? name) => Name = name;

    public static NoPublicConstructorTupleObject Create(string? name) => new NoPublicConstructorTupleObject(name);
}

public class NoPublicConstructorTests
{
    public static IEnumerable<object[]> NoPublicConstructorData()
    {
        yield return new object[] { NoPublicConstructorNamedObject.Create(-3) };
        yield return new object[] { NoPublicConstructorTupleObject.Create("Invalid") };
    }

    [Theory(DisplayName = "No Public Constructor Test")]
    [MemberData(nameof(NoPublicConstructorData))]
    public void NoPublicConstructorTest<T>(T source)
    {
        var builder = Generator.CreateAotBuilder();
        foreach (var i in NoPublicConstructorGeneratorContext.ConverterCreators)
            _ = builder.AddConverterCreator(i.Value);
        var generator = builder.Build();
        var converter = generator.GetConverter<T>();
        var buffer = converter.Encode(source);
        Assert.NotEmpty(buffer);

        var error = Assert.Throws<NotSupportedException>(() => converter.Decode(buffer));
        var message = $"No suitable constructor found, type: {typeof(T)}";
        Assert.Equal(message, error.Message);
    }
}
