namespace Mikodev.Binary.Tests.Converters;

using Mikodev.Binary.Tests.Contexts;
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
        Assert.Equal("BigIntegerConverter", converter.GetType().Name);
        Assert.Equal(0, converter.Length);
    }

    public static IEnumerable<object[]> DataNumber =>
    [
        [new BigInteger()],
        [new BigInteger(long.MaxValue)],
        [new BigInteger(long.MinValue)],
        [BigInteger.Parse("91389681247993671255432112000000")],
        [BigInteger.Parse("-90315837410896312071002088037140000")],
    ];

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

        ConverterTests.TestVariableEncodeDecodeMethods(converter, data);
    }
}
