namespace Mikodev.Binary.Tests.Converters.Primitive;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using Xunit;

public class LargeIntegerTests
{
    private static void BinaryIntegerBasicTest<T>(int size, T item, Converter<T> converter, string converterTypeName) where T : IBinaryInteger<T>
    {
        var converterType = converter.GetType();
        Assert.Equal(converterTypeName, converterType.Name);
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

    public static IEnumerable<object[]> Int128Data =>
    [
        [16, default(Int128)],
        [16, Int128.MinValue],
        [16, Int128.MaxValue],
        [16, Int128.Zero],
        [16, Int128.One],
        [16, Int128.NegativeOne],
        [16, Int128.Parse("11223344_55667788_99AABBCC_DDEEFF00".Replace("_", string.Empty), NumberStyles.HexNumber)],
    ];

    [Theory(DisplayName = "Int128 Converter Basic Info")]
    [MemberData(nameof(Int128Data))]
    public void Int128EncodeDecode(int size, Int128 item)
    {
        var converter = Generator.CreateDefault().GetConverter<Int128>();
        BinaryIntegerBasicTest(size, item, converter, "LittleEndianConverter`1");
    }

    public static IEnumerable<object[]> UInt128Data =>
    [
        [16, default(UInt128)],
        [16, UInt128.MinValue],
        [16, UInt128.MaxValue],
        [16, UInt128.Zero],
        [16, UInt128.One],
        [16, UInt128.Parse("11223344_55667788_99AABBCC_DDEEFF00".Replace("_", string.Empty), NumberStyles.HexNumber)],
    ];

    [Theory(DisplayName = "UInt128 Converter Basic Info")]
    [MemberData(nameof(UInt128Data))]
    public void UInt128EncodeDecode(int size, UInt128 item)
    {
        var converter = Generator.CreateDefault().GetConverter<UInt128>();
        BinaryIntegerBasicTest(size, item, converter, "LittleEndianConverter`1");
    }
}
