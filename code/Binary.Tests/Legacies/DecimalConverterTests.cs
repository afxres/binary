namespace Mikodev.Binary.Tests.Legacies;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class DecimalConverterTests
{
    [Fact(DisplayName = "Converter Type Name And Length")]
    public void GetConverter()
    {
        var converter = ReflectionExtensions.CreateInstance<Converter<decimal>>("DecimalConverter");
        Assert.Equal("Mikodev.Binary.Legacies.Instance.DecimalConverter", converter.GetType().FullName);
        Assert.Equal(sizeof(int) * 4, converter.Length);
    }

    public static IEnumerable<object[]> DataNumber => new List<object[]>
    {
        new object[] { decimal.MaxValue },
        new object[] { decimal.MinusOne },
        new object[] { decimal.MinValue },
        new object[] { decimal.One },
        new object[] { decimal.Zero },
    };

    [Theory(DisplayName = "Encode Decode")]
    [MemberData(nameof(DataNumber))]
    public void BasicTest(decimal number)
    {
        var converter = ReflectionExtensions.CreateInstance<Converter<decimal>>("DecimalConverter");
        var buffer = converter.Encode(number);
        Assert.Equal(16, buffer.Length);
        Assert.Equal(16, converter.Length);
        var bits = decimal.GetBits(number);
        var actualBits = Enumerable.Range(0, 4).Select(i => BinaryPrimitives.ReadInt32LittleEndian(buffer.AsSpan(i * 4, 4))).ToArray();
        Assert.Equal(bits, actualBits);
        var result = converter.Decode(buffer);
        Assert.Equal(number, result);
    }
}
