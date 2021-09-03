namespace Mikodev.Binary.Tests.Converters;

using System;
using System.Runtime.CompilerServices;
using Xunit;

public class GuidConverterTests
{
    [Fact(DisplayName = "Converter Type Name And Length")]
    public void GetConverter()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<Guid>();
        Assert.Equal("Mikodev.Binary.Converters.GuidConverter", converter.GetType().FullName);
        Assert.Equal(Unsafe.SizeOf<Guid>(), converter.Length);
    }
}
