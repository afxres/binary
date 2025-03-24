namespace Mikodev.Binary.Tests.Features;

using System;
using Xunit;

public class ConverterThrowTests
{
    private const string ConverterTypeName = "LittleEndianConverter`1";

    private const string NotEnoughBytesMessage = "Not enough bytes or byte sequence invalid.";

    private readonly IGenerator generator;

    public ConverterThrowTests()
    {
        var generator = Generator.CreateDefault();
        this.generator = generator;
    }

    [Theory(DisplayName = "Decode Not Enough Bytes")]
    [InlineData(1, 0)]
    [InlineData(1, 3)]
    [InlineData(1.0, 5)]
    [InlineData(2.0, 7)]
    public void DecodeNotEnoughBytes<T>(T data, int length)
    {
        var converter = this.generator.GetConverter<T>();
        Assert.Equal(ConverterTypeName, converter.GetType().Name);
        var origin = converter.Encode(data);
        var buffer = origin.AsSpan(0, length).ToArray();
        var alpha = Assert.Throws<ArgumentException>(() => converter.Decode(buffer));
        var bravo = Assert.Throws<ArgumentException>(() => converter.Decode(new ReadOnlySpan<byte>(buffer)));
        Assert.Null(alpha.ParamName);
        Assert.Null(bravo.ParamName);
        Assert.Equal(NotEnoughBytesMessage, alpha.Message);
        Assert.Equal(NotEnoughBytesMessage, bravo.Message);
    }

    [Theory(DisplayName = "Decode Auto Not Enough Bytes")]
    [InlineData(1, 0)]
    [InlineData(1, 3)]
    [InlineData(1.0, 5)]
    [InlineData(2.0, 7)]
    public void DecodeAutoNotEnoughBytes<T>(T data, int length)
    {
        var converter = this.generator.GetConverter<T>();
        Assert.Equal(ConverterTypeName, converter.GetType().Name);
        var origin = converter.Encode(data);
        var buffer = origin.AsSpan(0, length).ToArray();
        var error = Assert.Throws<ArgumentException>(() =>
        {
            var span = new ReadOnlySpan<byte>(buffer);
            _ = converter.DecodeAuto(ref span);
            return;
        });
        Assert.Null(error.ParamName);
        Assert.Equal(NotEnoughBytesMessage, error.Message);
    }

    [Theory(DisplayName = "Decode With Length Prefix Incorrect Prefix")]
    [InlineData(1, 0)]
    [InlineData(1, 3)]
    [InlineData(1.0, 5)]
    [InlineData(2.0, 7)]
    public void DecodeWithLengthPrefixIncorrectPrefix<T>(T data, int length)
    {
        var converter = this.generator.GetConverter<T>();
        Assert.Equal(ConverterTypeName, converter.GetType().Name);
        var origin = converter.Encode(data);
        var allocator = new Allocator();
        Converter.Encode(ref allocator, length);
        Allocator.Append(ref allocator, origin);
        var buffer = allocator.ToArray();
        var error = Assert.Throws<ArgumentException>(() =>
        {
            var span = new ReadOnlySpan<byte>(buffer);
            _ = converter.DecodeWithLengthPrefix(ref span);
            return;
        });
        Assert.Null(error.ParamName);
        Assert.Equal(NotEnoughBytesMessage, error.Message);
    }

    [Theory(DisplayName = "Decode With Length Prefix Not Enough Bytes")]
    [InlineData(1, 0)]
    [InlineData(1, 3)]
    [InlineData(1.0, 5)]
    [InlineData(2.0, 7)]
    public void DecodeWithLengthPrefixNotEnoughBytes<T>(T data, int length)
    {
        var converter = this.generator.GetConverter<T>();
        Assert.Equal(ConverterTypeName, converter.GetType().Name);
        var origin = converter.Encode(data);
        var allocator = new Allocator();
        Converter.Encode(ref allocator, converter.Length);
        Allocator.Append(ref allocator, origin.AsSpan(0, length));
        var buffer = allocator.ToArray();
        var error = Assert.Throws<ArgumentException>(() =>
        {
            var span = new ReadOnlySpan<byte>(buffer);
            _ = converter.DecodeWithLengthPrefix(ref span);
            return;
        });
        Assert.Null(error.ParamName);
        Assert.Equal(NotEnoughBytesMessage, error.Message);
    }
}
