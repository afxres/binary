namespace Mikodev.Binary.Tests.Contexts;

using System;
using Xunit;

public class GeneratorAotTests
{
    [Fact(DisplayName = "To String (debug)")]
    public void CreateAot()
    {
        var generator = Generator.CreateAot();
        Assert.Matches(@"Converter Count = 1, Converter Creator Count = \d+", generator.ToString());
    }

    [Fact(DisplayName = "Get Converter (not supported)")]
    public void GetConverterNotSupported()
    {
        var generator = Generator.CreateAot();
        var error = Assert.Throws<NotSupportedException>(generator.GetConverter<ValueType>);
        Assert.Equal($"Invalid type: {typeof(ValueType)}", error.Message);
    }
}
