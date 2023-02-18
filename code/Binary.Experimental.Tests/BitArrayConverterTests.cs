namespace Mikodev.Binary.Experimental.Tests;

using System;
using System.Collections;
using System.Linq;
using Xunit;

public class BitArrayConverterTests
{
    [Fact(DisplayName = "Null Value")]
    public void NullValue()
    {
        var converter = new BitArrayConverter();
        var buffer = converter.Encode(null);
        Assert.Empty(buffer);
        var result = converter.Decode(Array.Empty<byte>());
        Assert.Null(result);
    }

    [Theory(DisplayName = "Empty Value")]
    [InlineData(new byte[] { 0 })]
    [InlineData(new byte[] { 0x80, 0, 0, 0 })]
    public void EmptyValue(byte[] header)
    {
        var converter = new BitArrayConverter();
        var source = new BitArray(0);
        var buffer = converter.Encode(source);
        Assert.Equal(new byte[] { 0 }, buffer);
        var result = converter.Decode(header);
        Assert.NotNull(result);
        Assert.Equal(0, result.Length);
    }

    [Theory(DisplayName = "Decode Invalid Bytes")]
    [InlineData(new byte[] { 2 })]
    [InlineData(new byte[] { 7 })]
    [InlineData(new byte[] { 0x80, 0, 0, 1 })]
    [InlineData(new byte[] { 0x80, 33, 0, 0 })]
    public void InvalidBytes(byte[] bytes)
    {
        var converter = new BitArrayConverter();
        var error = Assert.Throws<ArgumentException>(() => converter.Decode(bytes));
        Assert.Null(error.ParamName);
        Assert.Equal($"Invalid header or not enough bytes, type: {typeof(BitArray)}", error.Message);
    }

    [Fact(DisplayName = "Encode Decode Random Data (bit length: 0..128, loop count: 16)")]
    public void EncodeDecodeRandomData()
    {
        const int LoopCount = 16;
        const int BitLength = 128;
        var converter = new BitArrayConverter();
        for (var loop = 0; loop < LoopCount; loop++)
        {
            var bytes = new byte[BitLength / 8];
            Random.Shared.NextBytes(bytes);
            for (var i = 0; i < BitLength; i++)
            {
                var source = new BitArray(bytes) { Length = i };
                var buffer = converter.Encode(source);
                var intent = new ReadOnlySpan<byte>(buffer);
                var header = Converter.Decode(ref intent);
                Assert.Equal((i % 8) is 0 ? 0 : (8 - (i & 7)), header);
                Assert.Equal((i + 7) / 8, intent.Length);
                var result = converter.Decode(buffer);
                Assert.NotNull(result);
                Assert.Equal(i, result.Length);
                Assert.Equal(source.Cast<bool>(), result.Cast<bool>());
            }
        }
    }
}
