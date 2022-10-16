namespace Mikodev.Binary.Tests.Converters;

using System;
using System.Collections.Generic;
using Xunit;

public class VersionConverterTests
{
    [Fact(DisplayName = "Converter Type Name And Length")]
    public void BasicInfo()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<Version>();
        Assert.Equal("Mikodev.Binary.Converters.VersionConverter", converter.GetType().FullName);
        Assert.Equal(0, converter.Length);
    }

    public static readonly IEnumerable<object?[]> DataVersion = new List<object?[]>
    {
        new object?[] { 0, null },
        new object?[] { 8, new Version() },
        new object?[] { 8, new Version(2, 4) },
        new object?[] { 12, new Version(2, 4, 8) },
        new object?[] { 16, new Version(2, 4, 8, 16) },
    };

    [Theory(DisplayName = "Encode Decode")]
    [MemberData(nameof(DataVersion))]
    public void BasicTest(int length, Version? data)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<Version?>();
        var buffer = converter.Encode(data);
        Assert.Equal(length, buffer.Length);
        var result = converter.Decode(buffer);
        Assert.Equal(data, result);

        var a = Allocator.Invoke(data, (ref Allocator allocator, Version? item) => converter.Encode(ref allocator, item));
        var b = Allocator.Invoke(data, (ref Allocator allocator, Version? item) => converter.EncodeAuto(ref allocator, item));
        var c = Allocator.Invoke(data, (ref Allocator allocator, Version? item) => converter.EncodeWithLengthPrefix(ref allocator, item));
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

    [Fact(DisplayName = "Null Value & Empty Bytes")]
    public void Null()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<Version?>();
        Assert.True(ReferenceEquals(Array.Empty<byte>(), converter.Encode(null)));
        Assert.Null(converter.Decode(Array.Empty<byte>()));
    }

    [Theory(DisplayName = "Byte Length Error")]
    [InlineData(1)]
    [InlineData(9)]
    [InlineData(13)]
    [InlineData(17)]
    public void LengthError(int length)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<Version?>();
        var buffer = new byte[length];
        var error = Assert.Throws<ArgumentException>(() => converter.Decode(buffer));
        Assert.Null(error.ParamName);
        Assert.Equal("Not enough bytes or byte sequence invalid.", error.Message);
    }
}
