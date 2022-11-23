namespace Mikodev.Binary.Tests.Features.Converters;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Runtime.CompilerServices;
using Xunit;

public class GuidConverterTests
{
    [Fact(DisplayName = "Converter Type Name And Length")]
    public void GetConverter()
    {
        var creator = ReflectionExtensions.CreateInstance<IConverterCreator>("RawConverterCreator");
        var converter = Assert.IsAssignableFrom<Converter<Guid>>(creator.GetConverter(null!, typeof(Guid)));
        Assert.Matches("RawConverter.*GuidRawConverter", converter.GetType().FullName);
        Assert.Equal(Unsafe.SizeOf<Guid>(), converter.Length);
    }

    [Theory(DisplayName = "Encode Decode")]
    [InlineData("47a87bd4-053c-47c5-b970-7e2164b167bc")]
    [InlineData("cca92bd5-e5af-4913-9f29-4d4ccfa77784")]
    public void BasicTest(string data)
    {
        var guid = Guid.Parse(data);
        var creator = ReflectionExtensions.CreateInstance<IConverterCreator>("RawConverterCreator");
        var converter = Assert.IsAssignableFrom<Converter<Guid>>(creator.GetConverter(null!, typeof(Guid)));
        var buffer = converter.Encode(guid);
        Assert.Equal(16, buffer.Length);
        Assert.Equal(16, converter.Length);
        var actual = new Guid(new ReadOnlySpan<byte>(buffer));
        Assert.Equal(guid, actual);
        var result = converter.Decode(buffer);
        Assert.Equal(guid, result);
    }

    [Theory(DisplayName = "Not Exactly Bytes")]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(15)]
    public void NotExactlyBytes(int length)
    {
        var creator = ReflectionExtensions.CreateInstance<IConverterCreator>("RawConverterCreator");
        var converter = Assert.IsAssignableFrom<Converter<Guid>>(creator.GetConverter(null!, typeof(Guid)));
        var buffer = new byte[length];
        var actual = Assert.Throws<ArgumentException>(() => converter.Decode(new ReadOnlySpan<byte>(buffer)));
        Assert.Equal("Not enough bytes or byte sequence invalid.", actual.Message);
    }
}
