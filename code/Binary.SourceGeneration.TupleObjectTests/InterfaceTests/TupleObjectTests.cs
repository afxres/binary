namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.InterfaceTests;

using Mikodev.Binary.Attributes;
using System;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<IShelf>]
public partial class TupleObjectGeneratorContext { }

[TupleObject]
public interface IShelf
{
    [TupleKey(0)]
    string? Id { get; set; }

    [TupleKey(1)]
    double Weight { get; set; }
}

public class Shelf : IShelf
{
    public string? Id { get; set; }

    public double Weight { get; set; }
}

public class TupleObjectTests
{
    [Fact(DisplayName = "Tuple Object Interface Encode Test")]
    public void TupleObjectInterfaceEncodeTest()
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(TupleObjectGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<IShelf>();

        var source = new Shelf { Id = "A-1", Weight = 100.1 };
        var buffer = converter.Encode(source);

        var error = Assert.Throws<NotSupportedException>(() => converter.Decode(buffer));
        var message = $"No suitable constructor found, type: {typeof(IShelf)}";
        Assert.Equal(message, error.Message);

        var span = new ReadOnlySpan<byte>(buffer);
        var id = generator.GetConverter<string>().DecodeAuto(ref span);
        var weight = generator.GetConverter<double>().DecodeAuto(ref span);
        Assert.Equal(0, span.Length);
        Assert.Equal("A-1", id);
        Assert.Equal(100.1, weight);
    }
}
