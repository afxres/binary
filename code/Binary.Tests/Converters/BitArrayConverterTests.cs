namespace Mikodev.Binary.Tests.Converters;

using System;
using System.Collections;
using System.Linq;
using Xunit;

public class BitArrayConverterTests
{
    [Fact(DisplayName = "Converter Type Name And Length")]
    public void GetConverter()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BitArray>();
        Assert.Equal("Mikodev.Binary.Converters.BitArrayConverter", converter.GetType().FullName);
        Assert.Equal(0, converter.Length);
    }

    [Fact(DisplayName = "Encode Decode Random Data")]
    public void BasicTest()
    {
        var random = new Random();
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BitArray>();

        for (var ignore = 0; ignore < 32; ignore++)
        {
            var buffer = new byte[128];
            random.NextBytes(buffer);
            for (var k = 0; k < 1024; k++)
            {
                var source = new BitArray(buffer) { Length = k };
                var encode = converter.Encode(source);
                var target = new ReadOnlySpan<byte>(encode);
                var padding = Converter.Decode(ref target);
                Assert.True(padding is >= 0 and <= 7);
                Assert.Equal((-k) & 7, padding);
                var actual = new byte[(k + 7) >> 3];
                source.CopyTo(actual, 0);
                Assert.True(new ReadOnlySpan<byte>(actual).SequenceEqual(target));

                var result = converter.Decode(encode);
                Assert.Equal(k, result.Count);
                Assert.Equal(source.Cast<bool>(), result.Cast<bool>());
            }
        }
    }

    [Fact(DisplayName = "Null Instance")]
    public void NullInstance()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BitArray?>();
        var encode = converter.Encode(null);
        Assert.True(ReferenceEquals(Array.Empty<byte>(), encode));
        var result = converter.Decode(Array.Empty<byte>());
        Assert.Null(result);
    }

    [Fact(DisplayName = "Empty Collection")]
    public void Empty()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BitArray>();
        var source = new BitArray(0);
        var encode = converter.Encode(source);
        Assert.Equal(new[] { (byte)0 }, encode);
        var result = converter.Decode(encode);
        Assert.Empty(result.Cast<bool>());
    }

    [Fact(DisplayName = "Empty Collection Compatible Byte Sequence")]
    public void EmptyCompatibles()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BitArray>();
        var a = new byte[] { 0 };
        var b = new byte[] { 0x80, 0, 0, 0 };
        var x = converter.Decode(a);
        var y = converter.Decode(b);
        Assert.Empty(x.Cast<bool>());
        Assert.Empty(y.Cast<bool>());
    }

    [Fact(DisplayName = "Large Array Encode")]
    public void LargeArrayEncode()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BitArray>();
        var source = new BitArray(int.MaxValue);
        var encode = converter.Encode(source);
        var target = new ReadOnlySpan<byte>(encode);
        var margin = Converter.Decode(ref target);
        Assert.Equal(1, margin);
        Assert.Equal(0x1000_0000, target.Length);
    }

    [Fact(DisplayName = "Large Array Decode")]
    public void LargeArrayDecode()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BitArray>();
        var buffer = new byte[0x1000_0001];
        buffer[0] = 1;
        var result = converter.Decode(buffer);
        Assert.Equal(int.MaxValue, result.Length);
    }

    [Fact(DisplayName = "Large Array Overflow")]
    public void LargeArrayOverflow()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BitArray>();
        var buffer = new byte[0x1000_0001];
        var error = Assert.Throws<OverflowException>(() => converter.Decode(buffer));
        Assert.Equal(new OverflowException().Message, error.Message);
    }

    [Theory(DisplayName = "Invalid Margin Info")]
    [InlineData(new byte[] { 8, 0 })]
    [InlineData(new byte[] { 127, 1, 2 })]
    [InlineData(new byte[] { 0x80, 0, 0, 8, 4 })]
    [InlineData(new byte[] { 0x80, 2, 0, 0, 2, 8 })]
    public void InvalidMarginInfo(byte[] buffer)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BitArray>();
        var error = Assert.Throws<ArgumentException>(() => converter.Decode(buffer));
        var target = new ReadOnlySpan<byte>(buffer);
        var margin = Converter.Decode(ref target);
        Assert.True((uint)margin >= 8U);
        Assert.True(target.Length is not 0);
        Assert.Null(error.ParamName);
        Assert.Equal("Not enough bytes or byte sequence invalid.", error.Message);
    }

    [Theory(DisplayName = "Not Enough Bytes")]
    [InlineData(new byte[] { 1 })]
    [InlineData(new byte[] { 7 })]
    [InlineData(new byte[] { 0x80, 0, 0, 2 })]
    [InlineData(new byte[] { 0x80, 0, 0, 5 })]
    public void NotEnoughBytes(byte[] buffer)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BitArray>();
        var error = Assert.Throws<ArgumentException>(() => converter.Decode(buffer));
        var target = new ReadOnlySpan<byte>(buffer);
        var margin = Converter.Decode(ref target);
        Assert.True((uint)margin <= 7U);
        Assert.True(target.Length is 0);
        Assert.Null(error.ParamName);
        Assert.Equal("Not enough bytes or byte sequence invalid.", error.Message);
    }
}
