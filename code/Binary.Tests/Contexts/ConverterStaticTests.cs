namespace Mikodev.Binary.Tests.Contexts;

using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

public class ConverterStaticTests
{
    [Fact(DisplayName = "Argument Null Test")]
    public void ArgumentNullTest()
    {
        var methods = new List<MethodInfo>
        {
            new Func<IConverter, Type>(Converter.GetGenericArgument).Method,
            new Func<IConverter, string, MethodInfo>(Converter.GetMethod).Method
        };
        Assert.All(methods, ArgumentTests.ArgumentNullExceptionTest);
    }

    [Theory(DisplayName = "Encode By Allocator Overflow Test")]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void EncodeByAllocatorOverflowTest(int number)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var allocator = new Allocator();
            Converter.Encode(ref allocator, number);
        });
        var method = new AllocatorAction<int>(Converter.Encode).Method;
        var parameters = method.GetParameters();
        Assert.Equal("number", error.ParamName);
        Assert.Equal("number", parameters[1].Name);
    }

    private delegate void Write(Span<byte> span, int number, out int written);

    [Theory(DisplayName = "Encode By Span Overflow Test")]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void EncodeBySpanOverflowTest(int number)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => Converter.Encode([], number, out var _));
        var method = new Write(Converter.Encode).Method;
        var parameters = method.GetParameters();
        Assert.Equal("number", error.ParamName);
        Assert.Equal("number", parameters[1].Name);
    }

    [Theory(DisplayName = "Encode Decode Integration Test")]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(127, 1)]
    [InlineData(128, 4)]
    [InlineData(int.MaxValue, 4)]
    public void EncodeDecodeIntegrationTest(int number, int bytesWritten)
    {
        var allocator = new Allocator();
        Converter.Encode(ref allocator, number);
        Assert.Equal(bytesWritten, allocator.Length);
        var span = allocator.AsSpan();
        var result = Converter.Decode(ref span);
        Assert.Equal(number, result);
        Assert.Equal(0, span.Length);

        var buffer = new byte[bytesWritten];
        Converter.Encode(buffer, number, out var actualWritten);
        Assert.Equal(bytesWritten, actualWritten);
        var resultSecond = Converter.Decode(buffer, out var actualRead);
        Assert.Equal(number, resultSecond);
        Assert.Equal(bytesWritten, actualRead);
    }

    [Theory(DisplayName = "Encode By Span Not Enough Bytes Test")]
    [InlineData(1, 0)]
    [InlineData(127, 0)]
    [InlineData(128, 0)]
    [InlineData(128, 1)]
    [InlineData(128, 2)]
    [InlineData(128, 3)]
    [InlineData(int.MaxValue, 3)]
    public void EncodeBySpanNotEnoughBytesTest(int number, int spanLength)
    {
        var error = Assert.Throws<ArgumentException>(() => Converter.Encode(new byte[spanLength], number, out var _));
        Assert.Null(error.ParamName);
        Assert.Equal("Not enough bytes to write.", error.Message);
    }

    [Theory(DisplayName = "Decode Read Write Not Enough Bytes Test")]
    [InlineData(128, 1)]
    [InlineData(128, 2)]
    [InlineData(128, 3)]
    [InlineData(255, 3)]
    public void DecodeReadWriteNotEnoughBytesTest(byte header, int spanLength)
    {
        var buffer = new byte[spanLength];
        buffer[0] = header;
        var error = Assert.Throws<ArgumentException>(() =>
        {
            var span = new ReadOnlySpan<byte>(buffer);
            _ = Converter.Decode(ref span);
        });
        Assert.Null(error.ParamName);
        Assert.Equal("Not enough bytes or byte sequence invalid.", error.Message);
    }

    [Theory(DisplayName = "Decode Read Only Not Enough Bytes Test")]
    [InlineData(128, 1)]
    [InlineData(128, 2)]
    [InlineData(128, 3)]
    [InlineData(255, 3)]
    public void DecodeReadOnlyNotEnoughBytesTest(byte header, int spanLength)
    {
        var buffer = new byte[spanLength];
        buffer[0] = header;
        var error = Assert.Throws<ArgumentException>(() => _ = Converter.Decode(buffer, out var _));
        Assert.Null(error.ParamName);
        Assert.Equal("Not enough bytes or byte sequence invalid.", error.Message);
    }

    [Fact(DisplayName = "Decode Read Write Empty Bytes Test")]
    public void DecodeReadWriteEmptyBytesTest()
    {
        var error = Assert.Throws<ArgumentException>(() =>
        {
            var span = new ReadOnlySpan<byte>();
            _ = Converter.Decode(ref span);
        });
        Assert.Null(error.ParamName);
        Assert.Equal("Not enough bytes or byte sequence invalid.", error.Message);
    }

    [Fact(DisplayName = "Decode Read Only Empty Bytes Test")]
    public void DecodeReaOnlyEmptyBytesTest()
    {
        var error = Assert.Throws<ArgumentException>(() => _ = Converter.Decode([], out var _));
        Assert.Null(error.ParamName);
        Assert.Equal("Not enough bytes or byte sequence invalid.", error.Message);
    }
}
