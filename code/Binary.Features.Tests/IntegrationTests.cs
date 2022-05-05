namespace Mikodev.Binary.Features.Tests;

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

public class IntegrationTests
{
    public static IEnumerable<object[]> SimpleObjectData => new List<object[]>
    {
        new object[] { 0x04, 0 },
        new object[] { 0x08, 2.3 },
        new object[] { 0x04, DateOnly.Parse("2001-02-03") },
        new object[] { 0x0A, DateTimeOffset.Parse("2020-02-02T11:22:33+04:00") },
        new object[] { 0x08, DateTime.Parse("2001-02-03T04:05:06") },
        new object[] { 0x10, decimal.Parse("2.71828") },
        new object[] { 0x10, Guid.Parse("f28a5581-c80d-4d66-84cf-790d48e877d1") },
        new object[] { 0x04, (Rune)'#' },
        new object[] { 0x08, TimeOnly.Parse("12:34:56") },
        new object[] { 0x08, TimeSpan.Parse("01:23:45.6789") },
    };

    [Theory(DisplayName = "Converter Info")]
    [MemberData(nameof(SimpleObjectData))]
    public void ConverterBasicInfo(int length, object data)
    {
        var generator = Generator
            .CreateDefaultBuilder()
            .AddPreviewFeaturesConverterCreators()
            .Build();
        var converter = generator.GetConverter(data.GetType());
        Assert.Equal("RawConverter`2", converter.GetType().Name);
        Assert.Equal(length, converter.Length);
        var buffer = converter.Encode(data);
        var result = converter.Decode(buffer);
        Assert.Equal(data, result);
    }

    public static IEnumerable<object[]> NumberData => new List<object[]>
    {
        new object[] { 0 },
        new object[] { int.MaxValue },
        new object[] { int.MinValue },
        new object[] { double.MaxValue },
        new object[] { double.MinValue },
        new object[] { double.NaN },
        new object[] { double.PositiveInfinity },
        new object[] { double.NegativeInfinity },
    };

    public static IEnumerable<object[]> DateOnlyData => new List<object[]>
    {
        new object[] { DateOnly.MinValue },
        new object[] { DateOnly.MaxValue },
        new object[] { DateOnly.Parse("2000-01-01") },
    };

    public static IEnumerable<object[]> DateTimeOffsetData => new List<object[]>
    {
        new object[] { DateTimeOffset.MinValue },
        new object[] { DateTimeOffset.MaxValue },
        new object[] { DateTimeOffset.UnixEpoch },
        new object[] { DateTimeOffset.Parse("2000-01-01T11:22:33+14:00") },
        new object[] { DateTimeOffset.Parse("2000-01-01T11:22:33-14:00") },
    };

    public static IEnumerable<object[]> DateTimeData => new List<object[]>
    {
        new object[] { DateTime.MinValue },
        new object[] { DateTime.MaxValue },
        new object[] { DateTime.UnixEpoch },
        new object[] { DateTime.Parse("2000-01-01T11:22:33") },
        new object[] { DateTime.Parse("2000-01-01T23:12:01") },
    };

    public static IEnumerable<object[]> GuidData => new List<object[]>
    {
        new object[] { Guid.Empty },
        new object[] { Guid.Parse("9b4bc529-e00d-4304-92e7-4366e0839078") },
        new object[] { Guid.Parse("600c8464-8279-4613-9b1a-dc048e250cc9") },
    };

    public static IEnumerable<object[]> RuneData => new List<object[]>
    {
        new object[] { Rune.ReplacementChar },
        new object[] { (Rune)'A' },
        new object[] { (Rune)'一' },
    };

    public static IEnumerable<object[]> TimeOnlyData => new List<object[]>
    {
        new object[] { TimeOnly.MaxValue },
        new object[] { TimeOnly.MinValue },
        new object[] { TimeOnly.Parse("20:48:00") },
    };

    public static IEnumerable<object[]> TimeSpanData => new List<object[]>
    {
        new object[] { TimeSpan.MaxValue },
        new object[] { TimeSpan.MinValue },
        new object[] { TimeSpan.Parse("22:10:24.4096") },
    };

    [Theory(DisplayName = "Encode Decode")]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(DateOnlyData))]
    [MemberData(nameof(DateTimeOffsetData))]
    [MemberData(nameof(DateTimeData))]
    [MemberData(nameof(GuidData))]
    [MemberData(nameof(RuneData))]
    [MemberData(nameof(TimeOnlyData))]
    [MemberData(nameof(TimeSpanData))]
    public void EncodeDecode<T>(T item)
    {
        var generator = Generator
           .CreateDefaultBuilder()
           .AddPreviewFeaturesConverterCreators()
           .Build();
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
    [MemberData(nameof(DateOnlyData))]
    [MemberData(nameof(DateTimeOffsetData))]
    [MemberData(nameof(DateTimeData))]
    [MemberData(nameof(GuidData))]
    [MemberData(nameof(RuneData))]
    [MemberData(nameof(TimeOnlyData))]
    [MemberData(nameof(TimeSpanData))]
    public void EncodeDecodeAuto<T>(T item)
    {
        var generator = Generator
           .CreateDefaultBuilder()
           .AddPreviewFeaturesConverterCreators()
           .Build();
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
    [MemberData(nameof(DateOnlyData))]
    [MemberData(nameof(DateTimeOffsetData))]
    [MemberData(nameof(DateTimeData))]
    [MemberData(nameof(GuidData))]
    [MemberData(nameof(RuneData))]
    [MemberData(nameof(TimeOnlyData))]
    [MemberData(nameof(TimeSpanData))]
    public void EncodeDecodeWithLengthPrefix<T>(T item)
    {
        var generator = Generator
           .CreateDefaultBuilder()
           .AddPreviewFeaturesConverterCreators()
           .Build();
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
