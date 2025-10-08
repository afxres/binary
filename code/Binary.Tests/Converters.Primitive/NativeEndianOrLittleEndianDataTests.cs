namespace Mikodev.Binary.Tests.Converters.Primitive;

using System;
using System.Collections.Generic;
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
        [0],
        [int.MaxValue],
        [int.MinValue],
        [double.MaxValue],
        [double.MinValue],
        [double.NaN],
        [double.PositiveInfinity],
        [double.NegativeInfinity],
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
