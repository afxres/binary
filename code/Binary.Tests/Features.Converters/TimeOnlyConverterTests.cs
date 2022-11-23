namespace Mikodev.Binary.Tests.Features.Converters;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Buffers.Binary;
using Xunit;

public class TimeOnlyConverterTests
{
    [Fact(DisplayName = "Converter Type Name And Length")]
    public void GetConverter()
    {
        var creator = ReflectionExtensions.CreateInstance<IConverterCreator>("RawConverterCreator");
        var converter = Assert.IsAssignableFrom<Converter<TimeOnly>>(creator.GetConverter(null!, typeof(TimeOnly)));
        Assert.Matches("RawConverter.*TimeOnlyRawConverter", converter.GetType().FullName);
        Assert.Equal(sizeof(long), converter.Length);
    }

    [Theory(DisplayName = "Encode Decode")]
    [InlineData("06:30:30")]
    [InlineData("18:00:00")]
    public void BasicTest(string data)
    {
        var time = TimeOnly.ParseExact(data, "HH:mm:ss");
        var creator = ReflectionExtensions.CreateInstance<IConverterCreator>("RawConverterCreator");
        var converter = Assert.IsAssignableFrom<Converter<TimeOnly>>(creator.GetConverter(null!, typeof(TimeOnly)));
        var buffer = converter.Encode(time);
        Assert.Equal(8, buffer.Length);
        Assert.Equal(8, converter.Length);
        var binary = BinaryPrimitives.ReadInt64LittleEndian(buffer);
        Assert.Equal(time.Ticks, binary);
        var result = converter.Decode(buffer);
        Assert.Equal(time, result);
    }
}
