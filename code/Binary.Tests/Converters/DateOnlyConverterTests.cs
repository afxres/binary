namespace Mikodev.Binary.Tests.Converters;

using System;
using System.Buffers.Binary;
using Xunit;

public class DateOnlyConverterTests
{
    [Theory(DisplayName = "Encode Decode")]
    [InlineData("2020-12-12")]
    [InlineData("1900-01-01")]
    public void BasicTest(string data)
    {
        var date = DateOnly.ParseExact(data, "yyyy-MM-dd");
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<DateOnly>();
        Assert.Equal("Mikodev.Binary.Converters.DateOnlyConverter", converter.GetType().FullName);
        var buffer = converter.Encode(date);
        Assert.Equal(4, buffer.Length);
        Assert.Equal(4, converter.Length);
        var binary = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        Assert.Equal(date.DayNumber, binary);
        var result = converter.Decode(buffer);
        Assert.Equal(date, result);
    }
}
