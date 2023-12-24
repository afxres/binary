namespace Mikodev.Binary.Experimental.Tests;

using System;
using System.Linq;
using Xunit;

public class LengthHeadedArrayConverterTests
{
    [Theory(DisplayName = "Encode Decode Test")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(128)]
    [InlineData(1027)]
    [InlineData(65537)]
    public void EncodeDecodeTest(int length)
    {
        var source = Enumerable.Range(0, length).ToArray();
        var generator = Generator.CreateAot();
        var converter = new LengthHeadedArrayConverter<int>(generator.GetConverter<int>());
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal(source, result);
    }

    [Theory(DisplayName = "Length Header Test")]
    [InlineData(0, 1)]
    [InlineData(127, 1)]
    [InlineData(128, 4)]
    [InlineData(65537, 4)]
    public void LengthHeaderTest(int length, int headerByteLength)
    {
        var source = Enumerable.Range(0, length).ToArray();
        var generator = Generator.CreateAot();
        var converter = new LengthHeadedArrayConverter<int>(generator.GetConverter<int>());
        var buffer = converter.Encode(source);
        var actual = Converter.Decode(buffer, out var bytesRead);
        Assert.Equal(length, actual);
        Assert.Equal(headerByteLength, bytesRead);
    }

    [Fact(DisplayName = "Null Array Test")]
    public void NullArrayTest()
    {
        var generator = Generator.CreateAot();
        var converter = new LengthHeadedArrayConverter<int>(generator.GetConverter<int>());
        var buffer = converter.Encode(null);
        Assert.Empty(buffer);
        var result = converter.Decode(Array.Empty<byte>());
        Assert.Null(result);
    }

    [Fact(DisplayName = "Empty Array Test")]
    public void EmptyArrayTest()
    {
        var generator = Generator.CreateAot();
        var converter = new LengthHeadedArrayConverter<int>(generator.GetConverter<int>());
        var buffer = converter.Encode([]);
        Assert.Equal([0], buffer);
        var a = converter.Decode(new byte[] { 0 });
        var b = converter.Decode(new byte[] { 0x80, 0, 0, 0 });
        Assert.NotNull(a);
        Assert.NotNull(b);
        Assert.Empty(a);
        Assert.Empty(b);
    }
}
