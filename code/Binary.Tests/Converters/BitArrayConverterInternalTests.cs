﻿namespace Mikodev.Binary.Tests.Converters;

using System;
using System.Linq;
using System.Reflection;
using Xunit;

public class BitArrayConverterInternalTests
{
    private delegate void EncodeFunction(Span<byte> target, ReadOnlySpan<int> source, int length);

    private delegate void DecodeFunction(Span<int> target, ReadOnlySpan<byte> source, int length);

    private readonly EncodeFunction encode;

    private readonly DecodeFunction decode;

    public BitArrayConverterInternalTests()
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BitArrayConverter");
        var encode = type.GetMethod("EncodeContents", BindingFlags.Static | BindingFlags.NonPublic);
        var decode = type.GetMethod("DecodeContents", BindingFlags.Static | BindingFlags.NonPublic);
        this.encode = (EncodeFunction)Delegate.CreateDelegate(typeof(EncodeFunction), encode ?? throw new Exception());
        this.decode = (DecodeFunction)Delegate.CreateDelegate(typeof(DecodeFunction), decode ?? throw new Exception());
    }

    [Theory(DisplayName = "Encode Trim Tail Bits")]
    [InlineData(new byte[] { 1 }, new int[] { unchecked((int)0xFFFF_FFFF) }, 1)]
    [InlineData(new byte[] { 127 }, new int[] { unchecked((int)0xFFFF_FFFF) }, 7)]
    [InlineData(new byte[] { 0x78, 0x56, 0x34, 0x12, 0xFF, 1 }, new int[] { 0x12345678, unchecked((int)0xFFFF_FFFF) }, 41)]
    [InlineData(new byte[] { 0 }, new int[] { unchecked((int)0xAAAA_AAAAU) }, 1)]
    [InlineData(new byte[] { 1 }, new int[] { unchecked((int)0x5555_5555U) }, 1)]
    [InlineData(new byte[] { 0b010 }, new int[] { unchecked((int)0xAAAA_AAAAU) }, 3)]
    [InlineData(new byte[] { 0b10101 }, new int[] { unchecked((int)0x5555_5555U) }, 5)]
    public void EncodeTrimTailBits(byte[] expected, int[] source, int length)
    {
        var actual = new byte[expected.Length];
        this.encode.Invoke(actual, source, length);
        Assert.Equal(expected, actual);
    }

    [Theory(DisplayName = "Decode Trim Tail Bits")]
    [InlineData(new int[] { 1 }, new byte[] { 0xFF }, 1)]
    [InlineData(new int[] { 0x001F_5566 }, new byte[] { 0x66, 0x55, 0xFF }, 21)]
    [InlineData(new int[] { 0x3344_5566, 0x0000_3FCC }, new byte[] { 0x66, 0x55, 0x44, 0x33, 0xCC, 0xFF }, 46)]
    [InlineData(new int[] { 0 }, new byte[] { 0xAA }, 1)]
    [InlineData(new int[] { 1 }, new byte[] { 0x55 }, 1)]
    [InlineData(new int[] { 0b01010 }, new byte[] { 0xAA }, 5)]
    [InlineData(new int[] { 0b1010101 }, new byte[] { 0x55 }, 7)]
    public void DecodeTrimTailBits(int[] expected, byte[] source, int length)
    {
        var actual = new int[expected.Length];
        this.decode.Invoke(actual, source, length);
        Assert.Equal(expected, actual);
    }

    [Theory(DisplayName = "Encode Bounds Checking")]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(7, 2, 63)]
    [InlineData(17, 4, 129)]
    public void EncodeBoundsChecking(int target, int source, int length)
    {
        var error = Assert.Throws<IndexOutOfRangeException>(() =>
        {
            var a = new byte[target];
            var b = new int[source];
            this.encode.Invoke(a, b, length);
        });
        Assert.Equal(new IndexOutOfRangeException().Message, error.Message);
    }

    [Theory(DisplayName = "Decode Bounds Checking")]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(2, 7, 63)]
    [InlineData(4, 17, 129)]
    public void DecodeBoundsChecking(int target, int source, int length)
    {
        var error = Assert.Throws<IndexOutOfRangeException>(() =>
        {
            var a = new int[target];
            var b = new byte[source];
            this.decode.Invoke(a, b, length);
        });
        Assert.Equal(new IndexOutOfRangeException().Message, error.Message);
    }

    [Theory(DisplayName = "Encode Slices Error")]
    [InlineData(0, 1, 33)]
    [InlineData(4, 2, 65)]
    public void EncodeSlicesError(int target, int source, int length)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var a = new byte[target];
            var b = new int[source];
            this.encode.Invoke(a, b, length);
        });
        Assert.Equal("length", error.ParamName);
    }

    [Theory(DisplayName = "Decode Slices Error")]
    [InlineData(1, 0, 33)]
    [InlineData(2, 4, 65)]
    public void DecodeSlicesError(int target, int source, int length)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var a = new int[target];
            var b = new byte[source];
            this.decode.Invoke(a, b, length);
        });
        Assert.Equal("length", error.ParamName);
    }
}
