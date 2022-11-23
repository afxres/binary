namespace Mikodev.Binary.Tests.Features.Converters;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Buffers.Binary;
using Xunit;

public class DateOnlyConverterTests
{
    [Fact(DisplayName = "Converter Type Name And Length")]
    public void GetConverter()
    {
        var creator = ReflectionExtensions.CreateInstance<IConverterCreator>("RawConverterCreator");
        var converter = Assert.IsAssignableFrom<Converter<DateOnly>>(creator.GetConverter(null!, typeof(DateOnly)));
        Assert.Matches("RawConverter.*DateOnlyRawConverter", converter.GetType().FullName);
        Assert.Equal(sizeof(int), converter.Length);
    }

    [Theory(DisplayName = "Encode Decode")]
    [InlineData("2020-12-12")]
    [InlineData("1900-01-01")]
    public void BasicTest(string data)
    {
        var date = DateOnly.ParseExact(data, "yyyy-MM-dd");
        var creator = ReflectionExtensions.CreateInstance<IConverterCreator>("RawConverterCreator");
        var converter = Assert.IsAssignableFrom<Converter<DateOnly>>(creator.GetConverter(null!, typeof(DateOnly)));
        var buffer = converter.Encode(date);
        Assert.Equal(4, buffer.Length);
        Assert.Equal(4, converter.Length);
        var binary = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        Assert.Equal(date.DayNumber, binary);
        var result = converter.Decode(buffer);
        Assert.Equal(date, result);
    }
}
