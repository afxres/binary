namespace Mikodev.Binary.Tests.Components;

using Mikodev.Binary.Components;
using System;
using System.Collections.Generic;
using Xunit;

public class TupleObjectTests
{
    private sealed class HideConverter : IConverter
    {
        public int Length => throw new NotSupportedException();

        public void Encode(ref Allocator allocator, object? item) => throw new NotSupportedException();

        public void EncodeAuto(ref Allocator allocator, object? item) => throw new NotSupportedException();

        public void EncodeWithLengthPrefix(ref Allocator allocator, object? item) => throw new NotSupportedException();

        public byte[] Encode(object? item) => throw new NotSupportedException();

        public object? Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

        public object? DecodeAuto(ref ReadOnlySpan<byte> span) => throw new NotSupportedException();

        public object? DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => throw new NotSupportedException();

        public object? Decode(byte[]? buffer) => throw new NotSupportedException();
    }

    private sealed class FakeConverter<T>(int length) : Converter<T>(length)
    {
        public override void Encode(ref Allocator allocator, T? item) => throw new NotImplementedException();

        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotImplementedException();
    }

    [Fact(DisplayName = "Argument Null Test")]
    public void ArgumentNullTest()
    {
        var methodInfo = new Func<IEnumerable<IConverter>, int>(TupleObject.GetConverterLength).Method;
        var error = Assert.Throws<ArgumentNullException>(() => TupleObject.GetConverterLength(null!));
        var parameters = methodInfo.GetParameters();
        Assert.Equal(parameters[0].Name, error.ParamName);
    }

    [Fact(DisplayName = "Overflow Test")]
    public void OverflowTest()
    {
        var converter = new FakeConverter<int>(0x4000_0000);
        var error = Assert.Throws<OverflowException>(() => TupleObject.GetConverterLength([converter, converter]));
        Assert.Equal(new OverflowException().Message, error.Message);
    }

    [Fact(DisplayName = "Sequence Null Test")]
    public void SequenceNullTest()
    {
        var methodInfo = new Func<IEnumerable<IConverter>, int>(TupleObject.GetConverterLength).Method;
        var error = Assert.Throws<ArgumentException>(() => TupleObject.GetConverterLength([null!]));
        var parameters = methodInfo.GetParameters();
        Assert.Equal(parameters[0].Name, error.ParamName);
        Assert.StartsWith("Sequence contains null or invalid element.", error.Message);
    }

    [Fact(DisplayName = "Sequence Invalid Test")]
    public void SequenceInvalidTest()
    {
        var methodInfo = new Func<IEnumerable<IConverter>, int>(TupleObject.GetConverterLength).Method;
        var error = Assert.Throws<ArgumentException>(() => TupleObject.GetConverterLength([new HideConverter()]));
        var parameters = methodInfo.GetParameters();
        Assert.Equal(parameters[0].Name, error.ParamName);
        Assert.StartsWith("Sequence contains null or invalid element.", error.Message);
    }

    [Fact(DisplayName = "Sequence Empty Test")]
    public void SequenceEmptyTest()
    {
        var methodInfo = new Func<IEnumerable<IConverter>, int>(TupleObject.GetConverterLength).Method;
        var error = Assert.Throws<ArgumentException>(() => TupleObject.GetConverterLength([]));
        var parameters = methodInfo.GetParameters();
        Assert.Equal(parameters[0].Name, error.ParamName);
        Assert.StartsWith("Sequence is empty.", error.Message);
    }
}
