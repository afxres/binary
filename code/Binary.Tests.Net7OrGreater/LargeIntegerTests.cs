namespace Mikodev.Binary.Tests.Net7OrGreater;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Xunit;

public class LargeIntegerTests
{
    private static void BinaryIntegerBasicTest<T>(int size, T item) where T : IBinaryInteger<T>
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var converterType = converter.GetType();
        Assert.Equal("RawConverter`2", converterType.Name);
        var genericArguments = converterType.GetGenericArguments();
        var rawConverterType = genericArguments.Last();
        var standardName = $"{typeof(T).Name}RawConverter";
        Assert.Equal(standardName, rawConverterType.Name);
        Assert.Equal(size, converter.Length);
        var buffer = converter.Encode(item);
        var result = converter.Decode(buffer);
        Assert.Equal(item, result);
        var expectedBuffer = new byte[size];
        var flag = item.TryWriteLittleEndian(expectedBuffer, out var bytesWritten);
        Assert.True(flag);
        Assert.Equal(size, bytesWritten);
        Assert.Equal(expectedBuffer, buffer);
    }

    public static IEnumerable<object[]> Int128Data => new List<object[]>
    {
        new object[] { 16, default(Int128) },
        new object[] { 16, Int128.MinValue },
        new object[] { 16, Int128.MaxValue },
        new object[] { 16, Int128.Zero },
        new object[] { 16, Int128.One },
        new object[] { 16, Int128.NegativeOne },
        new object[] { 16, Int128.Parse("11223344_55667788_99AABBCC_DDEEFF00".Replace("_", string.Empty), NumberStyles.HexNumber) },
    };

    [Theory(DisplayName = "Int128 Converter Basic Info")]
    [MemberData(nameof(Int128Data))]
    public void Int128EncodeDecode(int size, Int128 item)
    {
        BinaryIntegerBasicTest(size, item);
    }

    public static IEnumerable<object[]> UInt128Data => new List<object[]>
    {
        new object[] { 16, default(UInt128) },
        new object[] { 16, UInt128.MinValue },
        new object[] { 16, UInt128.MaxValue },
        new object[] { 16, UInt128.Zero },
        new object[] { 16, UInt128.One },
        new object[] { 16, UInt128.Parse("11223344_55667788_99AABBCC_DDEEFF00".Replace("_", string.Empty), NumberStyles.HexNumber) },
    };

    [Theory(DisplayName = "UInt128 Converter Basic Info")]
    [MemberData(nameof(UInt128Data))]
    public void UInt128EncodeDecode(int size, UInt128 item)
    {
        BinaryIntegerBasicTest(size, item);
    }
}
