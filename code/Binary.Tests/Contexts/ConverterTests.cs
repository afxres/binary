namespace Mikodev.Binary.Tests.Contexts;

using System;
using System.Buffers.Binary;
using System.Text;
using Xunit;

public class ConverterTests
{
    internal static void TestConstantEncodeDecodeMethods<T>(Converter<T> converter, T data)
    {
        var buffer = converter.Encode(data);

        var a = Allocator.Invoke(data, converter.Encode);
        var b = Allocator.Invoke(data, converter.EncodeAuto);
        var c = Allocator.Invoke(data, converter.EncodeWithLengthPrefix);
        Assert.Equal(buffer, a);
        Assert.Equal(a, b);
        Assert.Equal(converter.Length, a.Length);
        Assert.Equal(converter.Length, b.Length);

        var i = new ReadOnlySpan<byte>(a);
        var j = new ReadOnlySpan<byte>(b);
        var k = new ReadOnlySpan<byte>(c);
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

    internal static void TestVariableEncodeDecodeMethods<T>(Converter<T> converter, T data)
    {
        var buffer = converter.Encode(data);

        var a = Allocator.Invoke(data, converter.Encode);
        var b = Allocator.Invoke(data, converter.EncodeAuto);
        var c = Allocator.Invoke(data, converter.EncodeWithLengthPrefix);
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

    private delegate T DecodeDelegate<out T>(ReadOnlySpan<byte> span);

    private class CustomCallbackConverter<T>(int length) : Converter<T>(length)
    {
        public required DecodeDelegate<T> DecodeDelegate { get; init; }

        public required AllocatorAction<T?> EncodeDelegate { get; init; }

        public override T Decode(in ReadOnlySpan<byte> span) => DecodeDelegate.Invoke(span);

        public override void Encode(ref Allocator allocator, T? item) => EncodeDelegate.Invoke(ref allocator, item);
    }

    [Fact(DisplayName = "Simple Constant Converter")]
    public void SimpleConstantConverter()
    {
        var converter = new CustomCallbackConverter<int>(sizeof(int))
        {
            DecodeDelegate = BinaryPrimitives.ReadInt32LittleEndian,
            EncodeDelegate = (ref Allocator allocator, int data) => Allocator.Append(ref allocator, sizeof(int), data, BinaryPrimitives.WriteInt32LittleEndian),
        };

        var data = new[] { 1, 4, 16, int.MaxValue, int.MinValue, 65536, };
        foreach (var i in data)
            TestConstantEncodeDecodeMethods(converter, i);
        Assert.Equal(4, converter.Length);
    }

    [Fact(DisplayName = "Simple Variable Converter")]
    public void SimpleVariableConverter()
    {
        var converter = new CustomCallbackConverter<string>(0)
        {
            DecodeDelegate = Encoding.UTF8.GetString,
            EncodeDelegate = (ref Allocator allocator, string? data) => Allocator.Append(ref allocator, data, Encoding.UTF8),
        };

        var data = new[] { "Alpha", "C#", "Hello, world!", "今天天气不错" };
        foreach (var i in data)
            TestVariableEncodeDecodeMethods(converter, i);
        Assert.Equal(0, converter.Length);
    }
}
