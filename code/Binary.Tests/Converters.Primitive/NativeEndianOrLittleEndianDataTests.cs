namespace Mikodev.Binary.Tests.Converters.Primitive;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;

public class NativeEndianOrLittleEndianDataTests
{
    public static IEnumerable<object[]> SimpleObjectData =>
    [
        [0x04, DateOnly.Parse("2001-02-03")],
        [0x0A, DateTimeOffset.Parse("2020-02-02T11:22:33+04:00")],
        [0x08, DateTime.Parse("2001-02-03T04:05:06")],
        [0x10, decimal.Parse("2.71828")],
        [0x10, Guid.Parse("f28a5581-c80d-4d66-84cf-790d48e877d1")],
        [0x04, (Rune)'#'],
        [0x08, TimeOnly.Parse("12:34:56")],
        [0x08, TimeSpan.Parse("01:23:45.6789")],
    ];

    [Theory(DisplayName = "Converter Info")]
    [MemberData(nameof(SimpleObjectData))]
    public void ConverterBasicInfo(int length, object data)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter(data.GetType());
        var converterType = converter.GetType();
        Assert.Equal(data.GetType().Name + "Converter", converterType.Name);
        Assert.Equal(length, converter.Length);
        var buffer = converter.Encode(data);
        var result = converter.Decode(buffer);
        Assert.Equal(data, result);
    }

    public static IEnumerable<object[]> NumberData =>
    [
        // byte
        [(byte)0],
        [(byte)1],
        [(byte)127],
        [(byte)128],
        [(byte)254],
        [(byte)255],

        // sbyte
        [(sbyte)0],
        [(sbyte)1],
        [(sbyte)-1],
        [(sbyte)127],
        [(sbyte)-128],
        [(sbyte)-100],

        // short
        [(short)0],
        [(short)1],
        [(short)12345],
        [(short)-12345],
        [short.MaxValue],
        [short.MinValue],

        // ushort
        [(ushort)0],
        [(ushort)1],
        [(ushort)1000],
        [(ushort)32768],
        [ushort.MaxValue],
        [(ushort)50000],

        // int
        [0],
        [1],
        [-1],
        [int.MaxValue],
        [int.MinValue],
        [123456789],

        // uint
        [0u],
        [1u],
        [123456789u],
        [4000000000u],
        [uint.MaxValue],
        [2147483648u],

        // long
        [0L],
        [1L],
        [-1L],
        [long.MaxValue],
        [long.MinValue],
        [1234567890123456789L],

        // ulong
        [0UL],
        [1UL],
        [12345678901234567890UL],
        [9000000000000000000UL],
        [ulong.MaxValue],
        [18446744073709551615UL],

        // float
        [0F],
        [1F],
        [-1F],
        [float.MaxValue],
        [float.MinValue],
        [float.NaN],
        [float.PositiveInfinity],
        [3.14159F],

        // double
        [0D],
        [1D],
        [-1D],
        [double.MaxValue],
        [double.MinValue],
        [double.NaN],
        [double.PositiveInfinity],
        [3.141592653589793],

        // Half
        [Half.MinValue],
        [Half.MaxValue],
        [Half.NaN],
        [(Half)0F],
        [(Half)1F],
        [(Half)3.14F],
    ];

    public static IEnumerable<object[]> IndexData()
    {
        yield return [Index.Start];
        yield return [Index.End];
        yield return [Index.FromStart(2)];
        yield return [Index.FromEnd(3)];
    }

    public static IEnumerable<object[]> DateOnlyData =>
    [
        [DateOnly.MinValue],
        [DateOnly.MaxValue],
        [DateOnly.Parse("2000-01-01")],
    ];

    public static IEnumerable<object[]> DateTimeOffsetData =>
    [
        [DateTimeOffset.MinValue],
        [DateTimeOffset.MaxValue],
        [DateTimeOffset.UnixEpoch],
        [DateTimeOffset.Parse("2000-01-01T11:22:33+14:00")],
        [DateTimeOffset.Parse("2000-01-01T11:22:33-14:00")],
    ];

    public static IEnumerable<object[]> DateTimeData =>
    [
        [DateTime.MinValue],
        [DateTime.MaxValue],
        [DateTime.UnixEpoch],
        [DateTime.Parse("2000-01-01T11:22:33")],
        [DateTime.Parse("2000-01-01T23:12:01")],
    ];

    public static IEnumerable<object[]> GuidData =>
    [
        [Guid.Empty],
        [Guid.Parse("9b4bc529-e00d-4304-92e7-4366e0839078")],
        [Guid.Parse("600c8464-8279-4613-9b1a-dc048e250cc9")],
    ];

    public static IEnumerable<object[]> RuneData =>
    [
        [Rune.ReplacementChar],
        [(Rune)'A'],
        [(Rune)'一'],
    ];

    public static IEnumerable<object[]> TimeOnlyData =>
    [
        [TimeOnly.MaxValue],
        [TimeOnly.MinValue],
        [TimeOnly.Parse("20:48:00")],
    ];

    public static IEnumerable<object[]> TimeSpanData =>
    [
        [TimeSpan.MaxValue],
        [TimeSpan.MinValue],
        [TimeSpan.Parse("22:10:24.4096")],
    ];

#if NET11_0_OR_GREATER
    public static IEnumerable<object[]> BFloat16Data =>
    [
        [BFloat16.MaxValue],
        [BFloat16.MinValue],
        [BFloat16.Parse("3.14159")],
        [BFloat16.Parse("-3.14159")],
        [BFloat16.Parse("0")],
        [BFloat16.Parse("1.0")],
        [BFloat16.Parse("0.00097656")], // small normalized (~2^-10)
    ];

    public static IEnumerable<object[]> Decimal32Data =>
    [
        [Decimal32.MaxValue],
        [Decimal32.MinValue],
        [Decimal32.Parse("3.14159")],
        [Decimal32.Parse("-3.14159")],
        [Decimal32.Parse("0")],
        [Decimal32.Parse("1")],
        [Decimal32.Parse("1.2345e-6")],
    ];

    public static IEnumerable<object[]> Decimal64Data =>
    [
        [Decimal64.MaxValue],
        [Decimal64.MinValue],
        [Decimal64.Parse("3.14159")],
        [Decimal64.Parse("-3.14159")],
        [Decimal64.Parse("0")],
        [Decimal64.Parse("1")],
        [Decimal64.Parse("1.23456789012345")],
    ];

    public static IEnumerable<object[]> Decimal128Data =>
    [
        [Decimal128.MaxValue],
        [Decimal128.MinValue],
        [Decimal128.Parse("3.14159")],
        [Decimal128.Parse("-3.14159")],
        [Decimal128.Parse("0")],
        [Decimal128.Parse("1")],
        [Decimal128.Parse("123456789.123456789123456789")],
    ];
#endif

    [Theory(DisplayName = "Encode Decode")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(IndexData))]
    [MemberData(nameof(DateOnlyData))]
    [MemberData(nameof(DateTimeOffsetData))]
    [MemberData(nameof(DateTimeData))]
    [MemberData(nameof(GuidData))]
    [MemberData(nameof(RuneData))]
    [MemberData(nameof(TimeOnlyData))]
    [MemberData(nameof(TimeSpanData))]
#if NET11_0_OR_GREATER
    [MemberData(nameof(BFloat16Data))]
    [MemberData(nameof(Decimal32Data))]
    [MemberData(nameof(Decimal64Data))]
    [MemberData(nameof(Decimal128Data))]
#endif
    public void EncodeDecode<T>(T item)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var allocator = new Allocator();
        var buffer = converter.Encode(item);
        Assert.Equal(converter.Length, buffer.Length);
        converter.Encode(ref allocator, item);
        var span = allocator.AsSpan();
        Assert.Equal(buffer, span.ToArray());
        var result = converter.Decode(buffer);
        var second = converter.Decode(span);
        Assert.Equal(item, result);
        Assert.Equal(item, second);
        Assert.Equal(converter.Length, allocator.Length);
        Assert.NotEmpty(buffer);
    }

    [Theory(DisplayName = "Encode Decode Auto")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(IndexData))]
    [MemberData(nameof(DateOnlyData))]
    [MemberData(nameof(DateTimeOffsetData))]
    [MemberData(nameof(DateTimeData))]
    [MemberData(nameof(GuidData))]
    [MemberData(nameof(RuneData))]
    [MemberData(nameof(TimeOnlyData))]
    [MemberData(nameof(TimeSpanData))]
#if NET11_0_OR_GREATER
    [MemberData(nameof(BFloat16Data))]
    [MemberData(nameof(Decimal32Data))]
    [MemberData(nameof(Decimal64Data))]
    [MemberData(nameof(Decimal128Data))]
#endif
    public void EncodeDecodeAuto<T>(T item)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var allocator = new Allocator();
        var buffer = converter.Encode(item);
        Assert.Equal(converter.Length, buffer.Length);
        converter.EncodeAuto(ref allocator, item);
        var span = allocator.AsSpan();
        Assert.Equal(buffer, span.ToArray());
        var result = converter.DecodeAuto(ref span);
        Assert.Equal(0, span.Length);
        Assert.Equal(item, result);
        Assert.Equal(converter.Length, allocator.Length);
        Assert.NotEmpty(buffer);
    }

    [Theory(DisplayName = "Encode Decode With Length Prefix")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(IndexData))]
    [MemberData(nameof(DateOnlyData))]
    [MemberData(nameof(DateTimeOffsetData))]
    [MemberData(nameof(DateTimeData))]
    [MemberData(nameof(GuidData))]
    [MemberData(nameof(RuneData))]
    [MemberData(nameof(TimeOnlyData))]
    [MemberData(nameof(TimeSpanData))]
#if NET11_0_OR_GREATER
    [MemberData(nameof(BFloat16Data))]
    [MemberData(nameof(Decimal32Data))]
    [MemberData(nameof(Decimal64Data))]
    [MemberData(nameof(Decimal128Data))]
#endif
    public void EncodeDecodeWithLengthPrefix<T>(T item)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var allocator = new Allocator();
        var buffer = converter.Encode(item);
        Assert.Equal(converter.Length, buffer.Length);
        converter.EncodeWithLengthPrefix(ref allocator, item);
        var data = allocator.AsSpan();
        var prefix = Converter.Decode(ref data);
        Assert.Equal(buffer, data.ToArray());
        Assert.Equal(prefix, buffer.Length);
        var body = allocator.AsSpan();
        var result = converter.DecodeWithLengthPrefix(ref body);
        Assert.Equal(0, body.Length);
        Assert.Equal(item, result);
        Assert.Equal(converter.Length, data.Length);
        Assert.NotEmpty(buffer);
    }
}
