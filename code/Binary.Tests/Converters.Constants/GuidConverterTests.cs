namespace Mikodev.Binary.Tests.Converters.Constants;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Runtime.CompilerServices;
using Xunit;

public class GuidConverterTests
{
    [Fact(DisplayName = "Converter Type Name And Length")]
    public void GetConverter()
    {
        var converter = ReflectionExtensions.CreateInstance<Converter<Guid>>("GuidConverter");
        Assert.Equal("GuidConverter", converter.GetType().Name);
        Assert.Equal(Unsafe.SizeOf<Guid>(), converter.Length);
    }

    [Theory(DisplayName = "Encode Decode")]
    [InlineData("47a87bd4-053c-47c5-b970-7e2164b167bc")]
    [InlineData("cca92bd5-e5af-4913-9f29-4d4ccfa77784")]
    public void BasicTest(string data)
    {
        var guid = Guid.Parse(data);
        var converter = ReflectionExtensions.CreateInstance<Converter<Guid>>("GuidConverter");
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
        var converter = ReflectionExtensions.CreateInstance<Converter<Guid>>("GuidConverter");
        var buffer = new byte[length];
        var actual = Assert.Throws<ArgumentException>(() => converter.Decode(new ReadOnlySpan<byte>(buffer)));
        Assert.Equal("Not enough bytes or byte sequence invalid.", actual.Message);
    }
}
