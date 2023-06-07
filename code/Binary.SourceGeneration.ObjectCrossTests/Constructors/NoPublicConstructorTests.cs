namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.Constructors;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<NoPublicConstructorNamedObject>]
[SourceGeneratorInclude<NoPublicConstructorTupleObject>]
[SourceGeneratorInclude<NoPublicConstructorPlainObject>]
public partial class NoPublicConstructorGeneratorContext { }

[NamedObject]
public class NoPublicConstructorNamedObject
{
    [NamedKey("id")]
    public int Id { get; set; }

    private NoPublicConstructorNamedObject() => throw new NotSupportedException();

    internal NoPublicConstructorNamedObject(int id) => Id = id;
}

[TupleObject]
public class NoPublicConstructorTupleObject
{
    [TupleKey(0)]
    public string? Name { get; set; }

    private NoPublicConstructorTupleObject() => throw new NotSupportedException();

    internal NoPublicConstructorTupleObject(string? name) => Name = name;
}

public class NoPublicConstructorPlainObject
{
    public double Data;

    private NoPublicConstructorPlainObject() => throw new NotSupportedException();

    internal NoPublicConstructorPlainObject(double data) => this.Data = data;
}

public class NoPublicConstructorTests
{
    public static IEnumerable<object[]> NoPublicConstructorData()
    {
        yield return new object[] { new NoPublicConstructorNamedObject(-3) };
        yield return new object[] { new NoPublicConstructorTupleObject("Invalid") };
        yield return new object[] { new NoPublicConstructorPlainObject(6.6) };
    }

    [Theory(DisplayName = "No Public Constructor Test")]
    [MemberData(nameof(NoPublicConstructorData))]
    public void NoPublicConstructorTest<T>(T source)
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(NoPublicConstructorGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<T>();
        var buffer = converter.Encode(source);
        Assert.NotEmpty(buffer);

        var error = Assert.Throws<NotSupportedException>(() => converter.Decode(buffer));
        var message = $"No suitable constructor found, type: {typeof(T)}";
        Assert.Equal(message, error.Message);
    }
}
