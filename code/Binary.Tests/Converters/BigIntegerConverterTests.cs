namespace Mikodev.Binary.Tests.Converters;

using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

public class BigIntegerConverterTests
{
    [Fact(DisplayName = "Converter Type Name And Length")]
    public void GetConverter()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BigInteger>();
        Assert.Equal("Mikodev.Binary.Converters.BigIntegerConverter", converter.GetType().FullName);
        Assert.Equal(0, converter.Length);
    }

    public static IEnumerable<object[]> DataNumber => new List<object[]>
    {
        new object[] { default(BigInteger) },
        new object[] { new BigInteger() },
        new object[] { new BigInteger(0) },
        new object[] { new BigInteger(long.MaxValue) },
        new object[] { new BigInteger(long.MinValue) },
        new object[] { BigInteger.Parse("91389681247993671255432112000000") },
        new object[] { BigInteger.Parse("-90315837410896312071002088037140000") },
    };

    [Theory(DisplayName = "Encode Decode")]
    [MemberData(nameof(DataNumber))]
    public void BasicTest(BigInteger data)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BigInteger>();
        var buffer = converter.Encode(data);
        Assert.Equal(data.GetByteCount(isUnsigned: false), buffer.Length);
        Assert.Equal(data.ToByteArray(isUnsigned: false, isBigEndian: false), buffer);
        var actual = new BigInteger(buffer, isUnsigned: false, isBigEndian: false);
        var result = converter.Decode(buffer);
        Assert.Equal(actual, result);

        var a = Allocator.Invoke(data, (ref Allocator allocator, BigInteger item) => converter.Encode(ref allocator, item));
        var b = Allocator.Invoke(data, (ref Allocator allocator, BigInteger item) => converter.EncodeAuto(ref allocator, item));
        var c = Allocator.Invoke(data, (ref Allocator allocator, BigInteger item) => converter.EncodeWithLengthPrefix(ref allocator, item));
        Assert.Equal(buffer, a);
        Assert.Equal(b, c);

        var i = new ReadOnlySpan<byte>(a);
        var j = new ReadOnlySpan<byte>(b);
        var k = new ReadOnlySpan<byte>(c);
        Assert.False(j.IsEmpty);
        Assert.False(k.IsEmpty);

        var x = converter.Decode(in i);
        var y = converter.DecodeAuto(ref j);
        var z = converter.DecodeWithLengthPrefix(ref k);
        Assert.Equal(data, x);
        Assert.Equal(data, y);
        Assert.Equal(data, z);

        Assert.True(j.IsEmpty);
        Assert.True(k.IsEmpty);
    }
}
