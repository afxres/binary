namespace Mikodev.Binary.SourceGeneration.NamedObjectTests.InterfaceTests;

using Mikodev.Binary.Attributes;
using System;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<IStudent>]
public partial class NamedObjectGeneratorContext { }

[NamedObject]
public interface IStudent
{
    [NamedKey("age")]
    int Age { get; set; }

    [NamedKey("name")]
    string? Name { get; set; }
}

public class Student : IStudent
{
    public int Age { get; set; }

    public string? Name { get; set; }
}

public class NamedObjectTests
{
    [Fact(DisplayName = "Named Object Interface Encode Test")]
    public void NamedObjectInterfaceEncodeTest()
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(NamedObjectGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<IStudent>();

        var source = new Student { Age = 20, Name = "Bob" };
        var buffer = converter.Encode(source);

        var error = Assert.Throws<NotSupportedException>(() => converter.Decode(buffer));
        var message = $"No suitable constructor found, type: {typeof(IStudent)}";
        Assert.Equal(message, error.Message);

        var token = new Token(generator, buffer);
        Assert.Equal(2, token.Children.Count);
        Assert.Equal(20, token["age"].As<int>());
        Assert.Equal("Bob", token["name"].As<string>());
    }
}
