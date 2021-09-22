namespace Mikodev.Binary.Tests.Converters;

using System;
using System.Buffers.Binary;
using Xunit;

#if NET6_0_OR_GREATER
public class TimeOnlyConverterTests
{
    [Fact(DisplayName = "Converter Type Name And Length")]
    public void GetConverter()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<TimeOnly>();
        Assert.Equal("Mikodev.Binary.Converters.TimeOnlyConverter", converter.GetType().FullName);
        Assert.Equal(sizeof(long), converter.Length);
    }

    [Theory(DisplayName = "Encode Decode")]
    [InlineData("06:30:30")]
    [InlineData("18:00:00")]
    public void BasicTest(string data)
    {
        var time = TimeOnly.ParseExact(data, "HH:mm:ss");
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<TimeOnly>();
        var buffer = converter.Encode(time);
        Assert.Equal(8, buffer.Length);
        Assert.Equal(8, converter.Length);
        var binary = BinaryPrimitives.ReadInt64LittleEndian(buffer);
        Assert.Equal(time.Ticks, binary);
        var result = converter.Decode(buffer);
        Assert.Equal(time, result);
    }
}
#endif
