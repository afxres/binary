namespace Mikodev.Binary.SourceGeneration.NamedObjectTests.InterfaceTests;

using Mikodev.Binary.Attributes;
using System;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<IPerson>]
public partial class PlainObjectGeneratorContext { }

public interface IPerson
{
    int Id { get; set; }

    string? Name { get; set; }
}

public class Person : IPerson
{
    public int Id { get; set; }

    public string? Name { get; set; }
}

public class PlainObjectTests
{
    [Fact(DisplayName = "Plain Object Interface Encode Test")]
    public void PlainObjectInterfaceEncodeTest()
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(PlainObjectGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<IPerson>();

        var source = new Person { Id = 65536, Name = "Tom" };
        var buffer = converter.Encode(source);

        var error = Assert.Throws<NotSupportedException>(() => converter.Decode(buffer));
        var message = $"No suitable constructor found, type: {typeof(IPerson)}";
        Assert.Equal(message, error.Message);

        var token = new Token(generator, buffer);
        Assert.Equal(2, token.Children.Count);
        Assert.Equal(65536, token["Id"].As<int>());
        Assert.Equal("Tom", token["Name"].As<string>());
    }
}
