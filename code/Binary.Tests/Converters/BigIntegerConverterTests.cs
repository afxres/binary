﻿namespace Mikodev.Binary.Tests.Converters;

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
    public void BasicTest(BigInteger number)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BigInteger>();
        var buffer = converter.Encode(number);
        Assert.Equal(number.GetByteCount(isUnsigned: false), buffer.Length);
        Assert.Equal(number.ToByteArray(isUnsigned: false, isBigEndian: false), buffer);
        var actual = new BigInteger(buffer, isUnsigned: false, isBigEndian: false);
        var result = converter.Decode(buffer);
        Assert.Equal(actual, result);
    }
}